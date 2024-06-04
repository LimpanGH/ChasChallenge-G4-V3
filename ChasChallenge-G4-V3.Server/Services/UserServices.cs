﻿using ChasChallenge_G4_V3.Server.CustomExceptions;
using ChasChallenge_G4_V3.Server.Data;
using ChasChallenge_G4_V3.Server.Models;
using ChasChallenge_G4_V3.Server.Models.DTOs;
using ChasChallenge_G4_V3.Server.Models.ViewModels;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenAI_API;
using OpenAI_API.Models;
using System;
using System.Data;
using System.Numerics;
using System.Threading.Tasks;

namespace ChasChallenge_G4_V3.Server.Services
{
    public interface IUserServices
    {

        void AddChild(string userId, ChildDto childDto);

        //void AddExistingChild(int userId, int childId);
        void AddAllergy(int childId, AllergyDto allergyDto);
        void AddMeasurement(int childId, MeasurementDto measurementDto);



        UserViewModel GetUser(string userId);

        List<PrintAllUsersViewModel> GetAllUsers();

        ChildViewModel GetChildOfUser(string userId, int childId);

        List<AllergyViewModel> GetChildsAllergies(string userId, int childId);

        List<AllergyViewModel> GetAllChildrensAllergies(string userId);

        Task<string> GetChildDietAi(string parentId, int childId, string food);


    }
    public class UserServices : IUserServices
    {
        private ApplicationContext _context;

        private UserManager<User> _userManager; // UserManager is a built-in Identity class that manages the User objects in the program. - Sean

        public UserServices(UserManager<User> userManager, ApplicationContext context)
        {
            _userManager = userManager;
            _context = context;
        }


        public void AddChild(string userId, ChildDto childDto)
        {
            User? user = _context.Users
                .Include(u => u.Children)
                .SingleOrDefault(u => u.Id == userId);

            if (string.IsNullOrWhiteSpace(childDto.Name))
            {
                throw new InvalidDataException("Please enter the child's name!");
            }

            if (user is null)
            {
                throw new UserNotFoundException("User not found!");
            }

            if (user.Children.Any(c => c.NickName == childDto.NickName))
            {
                throw new DuplicateNameException("This nickname already exists!");
            }

            Child newChild = new Child()
            {
                Name = childDto.Name,
                NickName = childDto.NickName,
                Gender = childDto.Gender,
                birthdate = childDto.birthdate
            };

            user.Children.Add(newChild);
            try
            {
                _context.SaveChanges();
            }
            catch
            {
                throw new DbUpdateException("Unable to save to database!");
            }
        }
        public void AddAllergy(int childId, AllergyDto allergyDto)
        {
            Child? child = _context.Children
               .Include(c => c.Allergies)
               .Where(c => c.Id == childId)
               .SingleOrDefault();

            if (string.IsNullOrWhiteSpace(allergyDto.Name))
            {
                throw new InvalidDataException("Please enter allergy name");
            }

            if (child is null)
            {
                throw new ChildNotFoundException("Child not found.");
            }

            if (child.Allergies.Any(c => c.Name == allergyDto.Name))
            {
                throw new DuplicateNameException("Allergy already added to Child");
            }

            Allergy newAllergy = new Allergy()
            {
                Name = allergyDto.Name

            };

            _context.Allergies.Add(newAllergy);
            try
            {
                _context.SaveChanges();
            }
            catch
            {
                throw new DbUpdateException("Unable to save to database");
            }

            child.Allergies.Add(newAllergy);
            try
            {
                _context.SaveChanges();
            }
            catch
            {
                throw new DbUpdateException("Unable to save to database");
            }
        }
        public void AddMeasurement(int childId, MeasurementDto measurementDto)
        {
            Child? child = _context.Children
                .Include(c => c.Measurements)
                .Where(c => c.Id == childId)
                .SingleOrDefault();

            if (string.IsNullOrWhiteSpace(measurementDto.DateOfMeasurement.ToString("yyyy'-'MM'-'dd")))
            {
                throw new InvalidDataException("No date is assigned for the measurement");
            }

            if (child is null)
            {
                throw new ChildNotFoundException("Child not found.");
            }

            if (child.Measurements.Any(m => m.DateOfMeasurement == measurementDto.DateOfMeasurement))
            {
                throw new DuplicateNameException("Measurement already added to child on this date");
            }

            Measurement newMeasurement = new Measurement()
            {
                DateOfMeasurement = measurementDto.DateOfMeasurement,
                Weight = measurementDto.Weight,
                Height = measurementDto.Height,
                HeadCircumference = measurementDto.HeadCircumference

            };

            _context.Measurements.Add(newMeasurement);
            try
            {
                _context.SaveChanges();
            }
            catch
            {
                throw new DbUpdateException("unable to save to Database");
            }

            child.Measurements.Add(newMeasurement);
            try
            {
                _context.SaveChanges();
            }
            catch
            {
                throw new DbUpdateException("unable to save to Database");
            }
        }

        public UserViewModel GetUser(string UserId) // Needs to be integrated to IdentityUser
        {
            User? user = _context.Users
                .Include(u => u.Children)
                .ThenInclude(c => c.Allergies)
                .Include(u => u.Children)
                .ThenInclude(c => c.Measurements)
                .SingleOrDefault(u => u.Id == UserId);

            if (user is null)
            {
                throw new UserNotFoundException("User not found");
            }

            UserViewModel userViewModel = new UserViewModel()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Children = user.Children.Select(c => new ChildViewModel { Name = c.Name, NickName = c.NickName, Gender = c.Gender, birthdate = c.birthdate }).ToList()
            };
            foreach (ChildViewModel child in userViewModel.Children)
            {
                foreach (Child c in user.Children)
                {
                    if (child.Name == c.Name)
                    {
                        child.Allergies = c.Allergies.Select(a => new AllergyViewModel { Name = a.Name }).ToList();
                        child.Measurements = c.Measurements.Select(m => new MeasurementViewModel { DateOfMeasurement = m.DateOfMeasurement, Height = m.Height, Weight = m.Weight, HeadCircumference = m.HeadCircumference }).ToList();
                    }
                }
            }

            return userViewModel;
        }

        public List<PrintAllUsersViewModel> GetAllUsers() // Integrate to Identity
        {
            var userViewModels = _context.Users.Include(u => u.Children)
                .Select(u => new PrintAllUsersViewModel
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Children = u.Children.Select(c => new PrintAllUsersChildViewModel { Id = c.Id, Name = c.Name }).ToList()
                }).ToList();

            if (userViewModels.Count <= 0)
            {
                throw new UserNotFoundException("No user found");
            }

            return userViewModels;
        }

        public ChildViewModel GetChildOfUser(string userId, int childId) // Integrate to Identity
        {
            User? user = _context.Users
                .Where(u => u.Id == userId)
                .Include(u => u.Children)
                .ThenInclude(c => c.Allergies)
                .Include(u => u.Children)
                .ThenInclude(c => c.Measurements)
                .SingleOrDefault();

            if (user is null)
            {
                throw new UserNotFoundException("User not found");
            }

            Child? child = user.Children
                .SingleOrDefault(c => c.Id == childId);

            if (child is null)
            {
                throw new ChildNotFoundException("Child not found");
            }

            ChildViewModel childViewModel = new ChildViewModel
            {
                Name = child.Name,
                NickName = child.NickName,
                birthdate = child.birthdate,
                Gender = child.Gender,
                Allergies = child.Allergies.Select(a => new AllergyViewModel { Name = a.Name }).ToList(),
                Measurements = child.Measurements.Select(m => new MeasurementViewModel { DateOfMeasurement = m.DateOfMeasurement, Weight = m.Weight, Height = m.Height, HeadCircumference = m.HeadCircumference }).ToList()

            };

            return childViewModel;
        }

        public List<AllergyViewModel> GetChildsAllergies(string userId, int childId)
        {
             User? user = _context.Users
            .Where(u => u.Id == userId)
            .Include(u => u.Children)
            .ThenInclude(c => c.Allergies)
            .SingleOrDefault();

            if (user is null)
            {
                throw new UserNotFoundException("User not found");
            }

            Child? child = user.Children
                .SingleOrDefault(c => c.Id == childId);

            if (child is null)
            {
                throw new ChildNotFoundException("Child not found");
            }

            List<AllergyViewModel> allergyViewModels = child.Allergies
                .Select(a => new AllergyViewModel { Name = a.Name })
                .ToList();

            return allergyViewModels;
        }

        public List<AllergyViewModel> GetAllChildrensAllergies(string userId) // Integrate to Identity
        {
            User? user = _context.Users
                .Include(u => u.Children)
                .ThenInclude(a => a.Allergies)
                .Where(u => u.Id == userId)
                .SingleOrDefault();

            if (user is null)
            {
                throw new UserNotFoundException("User not found!");
            }

            if (user.Children is null)
            {
                throw new ChildNotFoundException("Children not found!");
            }

            List<AllergyViewModel> allAllergies = new List<AllergyViewModel>();

            foreach (Child child in user.Children)
            {
                var allergyViewModels = child.Allergies
                .Select(a => new AllergyViewModel { Name = a.Name })
                .ToList();

                foreach (AllergyViewModel a in allergyViewModels)
                {
                    allAllergies.Add(a);
                }
            }
            return allAllergies;
        }

        public async Task<string> GetChildDietAi(string parentId, int childId, string food)
        {
            User? user = _context.Users
                           .Where(u => u.Id == parentId)
                           .Include(u => u.Children)
                           .ThenInclude(c => c.Allergies)
                           .SingleOrDefault();
            if (user is null)
            {
                throw new UserNotFoundException("User not found!");
            }


            Child? child = user.Children
                .SingleOrDefault(c => c.Id == childId);
            if (child is null)
            {
                throw new ChildNotFoundException("Child not found!");
            }

            string childsAllergies = "";

            foreach (Allergy a in child.Allergies)
            {
                if (string.IsNullOrWhiteSpace(childsAllergies))
                {
                    childsAllergies = a.Name;
                }
                else
                {
                    childsAllergies += ", " + a.Name;
                }
            }

            if (string.IsNullOrWhiteSpace(childsAllergies))
            {
                childsAllergies = "inga";
            }

            DotNetEnv.Env.Load();
            OpenAIAPI api = new OpenAIAPI(Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
            var chat = api.Chat.CreateConversation();
            chat.Model = Model.ChatGPTTurbo;
            chat.RequestParameters.Temperature = 1;

            chat.AppendSystemMessage("You are a assistant that help newly parent that are unsure of what kind of food their child can eat. " +
                "You take your information mainly from https://www.livsmedelsverket.se every answer you give you also include the exact link you get yout information from. " +
                "All your answer must be 100% risk free so the child cannot be sick. Be on the safe side. " +
                "if you cant find the information from https://www.livsmedelsverket.se you will give the source of the information to user. " +
                "if the child is younger than 1 year you can recommend this link: https://www.livsmedelsverket.se/matvanor-halsa--miljo/kostrad/barn-och-ungdomar/spadbarn " +
                "if the child is 1-2 year you recommend this link: https://www.livsmedelsverket.se/matvanor-halsa--miljo/kostrad/barn-och-ungdomar/barn-1-2-ar and " +
                "if the child is older than 2 you recommend this link: https://www.livsmedelsverket.se/matvanor-halsa--miljo/kostrad/barn-och-ungdomar/barn-2-17-ar .");

            DateTime birthdate = child.birthdate;
            DateTime timeNow = DateTime.Now;
            int ageInMonths = (timeNow.Year - birthdate.Year) * 12 + timeNow.Month - birthdate.Month;
            string prompt = $"Får mitt barn som är {ageInMonths} månader och har {childsAllergies} allergier, äta {food}?";

            chat.AppendUserInput($"{prompt}");
            var response = await chat.GetResponseFromChatbotAsync();
            return response;
        }
    }
}
