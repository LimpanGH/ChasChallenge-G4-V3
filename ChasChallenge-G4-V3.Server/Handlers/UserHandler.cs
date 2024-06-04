﻿using ChasChallenge_G4_V3.Server.CustomExceptions;
using ChasChallenge_G4_V3.Server.Models.DTOs;
using ChasChallenge_G4_V3.Server.Models.ViewModels;
using ChasChallenge_G4_V3.Server.Services;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ChasChallenge_G4_V3.Server.Handlers
{
    //Aldor nämnde att dessa inte borde vara static i lektionen 16/4? kika på det.
    public class UserHandler
    {
          
        public static IResult AddChild(IUserServices userServices, string userId, ChildDto childDto)
        {
            try
            {
                userServices.AddChild(userId, childDto);
                return Results.Ok();
            }
            catch (InvalidDataException ex)
            {
                return Results.BadRequest(new { message = ex.Message});
            }
            catch (UserNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message});
            }
            catch (DuplicateNameException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
            catch(DbUpdateException ex)
            {
                return Results.BadRequest(new { message = ex.Message});
            }
        }

        /*
        public IResult AddExistingChild(IUserServices userServices, int userId, int childId)
        {
            try
            {
                userServices.AddExistingChild(userId, childId);
            }
            catch
            {
                return Result.BadRequest();
            }

        }
        */

        public static IResult AddAllergy(IUserServices userServices, int childId, AllergyDto allergydto)
        {
            try
            {
                userServices.AddAllergy(childId, allergydto);
                return Results.Ok();
            }
            catch (InvalidDataException ex)
            {
                return Results.BadRequest(new { message = ex.Message});
            }
            catch(ChildNotFoundException ex)
            {
                return Results.NotFound(new {message = ex.Message});
            }
            catch(DuplicateNameException ex)
            {
                return Results.BadRequest(new {message = ex.Message});
            }
            catch(DbUpdateException ex)
            {
                return Results.BadRequest(new {messsage = ex.Message});
            }
        }

        public static IResult AddMeasurement(IUserServices userServices, int childId, MeasurementDto measurementDto)
        {
            try
            {
                userServices.AddMeasurement(childId, measurementDto);
                return Results.Ok();
            }
            catch (InvalidDataException ex)
            {
                return Results.BadRequest(new { message = ex.Message});
            }
            catch(ChildNotFoundException ex)
            {
                return Results.NotFound(new {messsage = ex.Message});
            }
            catch(DuplicateNameException ex)
            {
                return Results.BadRequest(new { message = ex.Message});
            }
            catch(DbUpdateException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        }

        /*---------------------------------------- GETS -------------------------------------*/

        public static IResult GetUser(IUserServices userServices, string userId)
        {
            try
            {
                var user = userServices.GetUser(userId);
                return Results.Json(user);
            }
            catch (UserNotFoundException ex)
            {
                return Results.NotFound(new  {message = ex.Message});
            }
        }

        public static IResult GetAllUsers(IUserServices userServices)
        {
            try
            {
                var users = userServices.GetAllUsers();
                return Results.Json(users);
            }
            catch (UserNotFoundException ex)
            {
                return Results.NotFound( new {message = ex.Message});
            }
        }

        public static IResult GetChildofUser(IUserServices userServices, string userId, int childId)
        {
            try
            {
                var child = userServices.GetChildOfUser(userId, childId);
                return Results.Json(child);
            }
            catch (UserNotFoundException ex)
            {
                return Results.NotFound( new {message = ex.Message});
            }
            catch (ChildNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        }

        public static IResult GetChildAllergies(IUserServices userServices, string userId, int childId)
        {
            try
            {
                var allergies = userServices.GetChildsAllergies(userId, childId);
                return Results.Json(allergies);
            }
            catch (UserNotFoundException ex)
            {
                return Results.NotFound(new {message = ex.Message});
            }
            catch (ChildNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        }

        public static IResult GetAllChildrensAllergies(IUserServices userServices, string userId)
        {
            try
            {
                var allergies = userServices.GetAllChildrensAllergies(userId);
                return Results.Json(allergies);
            }
            catch (UserNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (ChildNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        }

        public static async Task<IResult> GetChildDietAi(IUserServices userServices, string userId, int childId, string food)
        {
            try
            {
                var childDiet = await userServices.GetChildDietAi(userId, childId, food);
                return Results.Json(childDiet);
            }
            catch (UserNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch(ChildNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        }
    }
}
