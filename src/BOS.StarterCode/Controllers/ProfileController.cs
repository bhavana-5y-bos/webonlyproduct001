using BOS.Auth.Client;
using BOS.StarterCode.Helpers;
using BOS.StarterCode.Models;
using BOS.StarterCode.Models.BOSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace BOS.StarterCode.Controllers
{
    [Authorize(Policy = "IsAuthenticated")]
    public class ProfileController : Controller
    {
        private readonly IAuthClient _bosAuthClient;

        private Logger Logger;

        public ProfileController(IAuthClient authClient)
        {
            _bosAuthClient = authClient;
            Logger = new Logger();
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Returns the view where the user can view and update his profile
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            return View(await GetPageData());
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Trggers when the "Change Password" button is clicked
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangePassword()
        {
            return View("ChangePassword");
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggers when the user enters the new password to be saved in the database
        /// </summary>
        /// <param name="passwordObj"></param>
        /// <returns></returns>
        public async Task<ActionResult> UpdatePassword(ChangePassword passwordObj)
        {
            try
            {
                if (passwordObj != null)
                {
                    string userId = User.FindFirst(c => c.Type == "UserId").Value.ToString();
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var response = await _bosAuthClient.ChangePasswordAsync(Guid.Parse(userId), passwordObj.CurrentPassword, passwordObj.NewPassword);
                        if (response != null && response.IsSuccessStatusCode)
                        {
                            ViewBag.Message = "Password updated successfully";
                            return View("Index", await GetPageData());
                        }
                        else
                        {
                            ModelState.AddModelError("CustomError", response.BOSErrors[0].Message);
                            return View("ChangePassword");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("CustomError", "Your session seems to have expired. Please login again");
                        return View("ChangePassword");
                    }
                }
                else
                {
                    ModelState.AddModelError("CustomError", "The information sent was inaccurate. Please try again.");
                    return View("ChangePassword");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Profile", "UpdatePassword", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggered when the user sends across the information to be updated in the database
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> UpdateProfileInfo([FromBody]JObject data)
        {
            try
            {
                if (data != null)
                {
                    User user = new User
                    {
                        Id = Guid.Parse(User.FindFirst(c => c.Type == "UserId").Value.ToString()),
                        CreatedOn = DateTime.UtcNow,
                        Deleted = false,
                        Email = data["Email"].ToString(),
                        FirstName = data["FirstName"].ToString(),
                        LastModifiedOn = DateTime.UtcNow,
                        LastName = data["LastName"].ToString(),
                        Username = User.FindFirst(c => c.Type == "Username").Value.ToString()
                    };

                    var extendUserResponse = await _bosAuthClient.ExtendUserAsync(user);
                    if (extendUserResponse != null && extendUserResponse.IsSuccessStatusCode)
                    {
                        return "Your inforamtion has been updated successfully";
                    }
                    else
                    {
                        return extendUserResponse.BOSErrors[0].Message;
                    }
                }
                else
                {
                    return "The data inputted was inaccurate. Please try again.";
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Profile", "UpdatePassword", ex);
                return ex.Message;
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Is triggered when the user clicks "Update Username" after entering the new username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> UpdateUsername([FromBody]string username)
        {
            try
            {
                if (!User.FindFirst(c => c.Type == "Username").Value.ToString().Equals(username))
                {
                    string userId = User.FindFirst(c => c.Type == "UserId").Value.ToString();

                    var updatedUsernameResponse = await _bosAuthClient.UpdateUsernameAsync(Guid.Parse(userId), username);
                    if (updatedUsernameResponse.IsSuccessStatusCode)
                    {
                        return "Username updated successfully";
                    }
                    else
                    {
                        return "Unable to update username at this time. Please try again later.";
                    }
                }
                else
                {
                    return "No change to the username";
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Auth", "RegisterUser", ex);
                return ex.Message;
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Fetches the data that is required for rendering on the page 
        /// </summary>
        /// <returns></returns>
        private async Task<dynamic> GetPageData()
        {
            try
            {
                var moduleOperations = HttpContext.Session.GetObject<List<Module>>("ModuleOperations");
                Guid currentModuleId = new Guid();
                try
                {
                    currentModuleId = moduleOperations.Where(i => i.Code == "MYPFL").Select(i => i.Id).ToList()[0];
                }
                catch (ArgumentNullException)
                {
                    currentModuleId = Guid.Empty;
                }

                var currentOperations = moduleOperations.Where(i => i.Id == currentModuleId).Select(i => i.Operations).ToList()[0];
                string operationsString = String.Join(",", currentOperations.Select(i => i.Code));

                dynamic model = new ExpandoObject();
                model.ModuleOperations = HttpContext.Session.GetObject<List<Module>>("ModuleOperations");
                model.Operations = operationsString;
                model.CurrentModuleId = currentModuleId;
                model.Initials = User.FindFirst(c => c.Type == "Initials").Value.ToString();

                if (User.FindFirst(c => c.Type == "Username") != null || User.FindFirst(c => c.Type == "Role") != null)
                {
                    model.Username = User.FindFirst(c => c.Type == "Username").Value.ToString();
                    model.Roles = User.FindFirst(c => c.Type == "Role").Value.ToString();

                    string userId = User.FindFirst(c => c.Type == "UserId").Value.ToString();
                    var userInfo = await _bosAuthClient.GetUserByIdWithRolesAsync<User>(Guid.Parse(userId));
                    if (userInfo.IsSuccessStatusCode)
                    {
                        model.UserInfo = userInfo.User;
                    }

                    var availableRoles = await _bosAuthClient.GetRolesAsync<Role>();
                    if (availableRoles.IsSuccessStatusCode)
                    {
                        model.AvailableRoles = availableRoles.Roles;
                    }
                }
                return model;
            }
            catch (Exception ex)
            {
                Logger.LogException("Auth", "RegisterUser", ex);
                return null;
            }
        }
    }
}