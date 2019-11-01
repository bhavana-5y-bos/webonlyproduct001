using BOS.Auth.Client;
using BOS.Auth.Client.ClientModels;
using BOS.Email.Client;
using BOS.Email.Client.ClientModels;
using BOS.StarterCode.Helpers;
using BOS.StarterCode.Models.BOSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace BOS.StarterCode.Controllers
{
    [Authorize(Policy = "IsAuthenticated")]
    public class UsersController : Controller
    {
        private readonly IAuthClient _bosAuthClient;
        private readonly IEmailClient _bosEmailClient;
        private readonly IConfiguration _configuration;

        private Logger Logger;

        public UsersController(IAuthClient authClient, IEmailClient bosEmailClient, IConfiguration configuration)
        {
            _bosAuthClient = authClient;
            _bosEmailClient = bosEmailClient;
            _configuration = configuration;

            Logger = new Logger();
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Returns the View that lists all the users
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            return View(await GetPageData());
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Is triggered when the 'Add User' button is clicked. Returns the view with the form to add a new user.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> AddNewUser()
        {
            try
            {
                dynamic model = new ExpandoObject();

                var availableRoles = await _bosAuthClient.GetRolesAsync<Role>();
                if (availableRoles != null && availableRoles.IsSuccessStatusCode)
                {
                    model.AvailableRoles = availableRoles.Roles;
                }
                return View("AddUser", model);
            }
            catch (Exception ex)
            {
                Logger.LogException("Users", "AddNewUser", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Is triggered when the 'Edit' link is clicked. Returns the view with the form to edit the selected user, with the information pre-filled.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditUser(string userId)
        {
            try
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    dynamic model = new ExpandoObject();
                    StringConversion stringConversion = new StringConversion();
                    string actualUserId = stringConversion.DecryptString(userId);
                    var userInfo = await _bosAuthClient.GetUserByIdWithRolesAsync<User>(Guid.Parse(actualUserId));
                    if (userInfo != null && userInfo.IsSuccessStatusCode)
                    {
                        userInfo.User.UpdatedId = userId;
                        model.UserInfo = userInfo.User;
                    }

                    List<string> rolesList = new List<string>();
                    foreach (UserRole role in userInfo.User.Roles)
                    {
                        rolesList.Add(role.Role.Name);
                    }
                    model.RolesList = rolesList;
                    var availableRoles = await _bosAuthClient.GetRolesAsync<Role>();
                    if (availableRoles != null && availableRoles.IsSuccessStatusCode)
                    {
                        model.AvailableRoles = availableRoles.Roles;
                    }

                    return View("EditUser", model);
                }
                else
                {
                    ModelState.AddModelError("CustomError", "The selected user has inaccurate id. Please try again.");
                    return View("Index", await GetPageData());
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Users", "EditUser", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Is triggered when the 'Save' button is clicked with the details of the new user
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> AddUser([FromBody] JObject data)
        {
            try
            {
                if (data != null)
                {
                    User userObj = data["User"]?.ToObject<User>();
                    List<Role> roleList = data["Roles"]?.ToObject<List<Role>>();
                    bool isEmailToSend = Convert.ToBoolean(data["IsEmailToSend"]?.ToString());
                    string password = data["Password"]?.ToString();
                    if (isEmailToSend)
                    {
                        password = CreatePassword();
                    }

                    if (userObj != null)
                    {
                        var result = await _bosAuthClient.AddNewUserAsync<BOSUser>(userObj.Username, userObj.Email, password);
                        if (result != null && result.IsSuccessStatusCode)
                        {
                            User user = userObj;
                            user.Id = result.User.Id;

                            var extendUserResponse = await _bosAuthClient.ExtendUserAsync(user);
                            if (extendUserResponse != null && extendUserResponse.IsSuccessStatusCode)
                            {
                                var roleResponse = await _bosAuthClient.AssociateUserToMultipleRolesAsync(result.User.Id, roleList);
                                if (roleResponse != null && roleResponse.IsSuccessStatusCode)
                                {
                                    if (isEmailToSend)
                                    {
                                        var slugResponse = await _bosAuthClient.CreateSlugAsync(userObj.Email);
                                        if (slugResponse != null && slugResponse.IsSuccessStatusCode)
                                        {
                                            var slug = slugResponse.Slug;

                                            Models.BOSModels.Email emailObj = new Models.BOSModels.Email
                                            {
                                                Deleted = false,
                                                From = new From
                                                {
                                                    Email = "startercode@bosframework.com",
                                                    Name = "StarterCode Team",
                                                },
                                                To = new List<To>
                                                {
                                                    new To
                                                    {
                                                        Email = userObj.Email,
                                                        Name = userObj.FirstName + " " + userObj.LastName
                                                    }
                                                }
                                            };
                                            var templateResponse = await _bosEmailClient.GetTemplateAsync<Template>();
                                            if (templateResponse != null && templateResponse.IsSuccessStatusCode)
                                            {
                                                emailObj.TemplateId = templateResponse.Templates.Where(i => i.Name == "UserAddedBySuperAdmin").Select(i => i.Id).ToList()[0];
                                            }
                                            else
                                            {
                                                ModelState.AddModelError("CustomError", "Sorry! We could not send you an email. Please try again later");
                                                return View("Index", await GetPageData());
                                            }

                                            var spResponse = await _bosEmailClient.GetServiceProviderAsync<ServiceProvider>();
                                            if (spResponse != null && spResponse.IsSuccessStatusCode)
                                            {
                                                emailObj.ServiceProviderId = spResponse.ServiceProvider[0].Id;
                                            }
                                            else
                                            {
                                                ModelState.AddModelError("CustomError", "Sorry! We could not send you an email. Please try again later");
                                                return View("Index", await GetPageData());
                                            }

                                            emailObj.Substitutions = new List<Substitution>();
                                            emailObj.Substitutions.Add(new Substitution { Key = "companyUrl", Value = _configuration["PublicUrl"] });
                                            emailObj.Substitutions.Add(new Substitution { Key = "companyLogo", Value = _configuration["PublicUrl"] + "/wwwroot/images/logo.png" });
                                            emailObj.Substitutions.Add(new Substitution { Key = "applicationName", Value = _configuration["ApplicationName"] });
                                            emailObj.Substitutions.Add(new Substitution { Key = "applicationUrl", Value = _configuration["PublicUrl"] + "/Password/Reset?slug=" + slug.Value });
                                            emailObj.Substitutions.Add(new Substitution { Key = "emailAddress", Value = user.Email });
                                            emailObj.Substitutions.Add(new Substitution { Key = "password", Value = "" });
                                            emailObj.Substitutions.Add(new Substitution { Key = "thanksCredits", Value = "Team StarterCode" });

                                            var emailResponse = await _bosEmailClient.SendEmailAsync<IEmail>(emailObj);
                                            if (!emailResponse.IsSuccessStatusCode)
                                            {
                                                ModelState.AddModelError("CustomError", emailResponse.BOSErrors[0].Message);
                                            }
                                        }
                                    }
                                    return "User added successfully";
                                }
                            }
                            return result != null ? result.BOSErrors[0].Message : "We are unable to add users at this time. Please try again.";
                        }
                        else
                        {
                            return result != null ? result.BOSErrors[0].Message : "We are unable to add users at this time. Please try again.";
                        }
                    }
                    else
                    {
                        return "User data cannot be null. Please check and try again.";
                    }
                }
                else
                {
                    return "The data inputted is inaccurate. Please try again.";
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Users", "AddUser", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Is triggered after the confirmation on the UI to delete the selected user.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> DeleteUser([FromBody]string userId)
        {
            try
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    StringConversion stringConversion = new StringConversion();
                    string actualUserId = stringConversion.DecryptString(userId);

                    var response = await _bosAuthClient.DeleteUserAsync(Guid.Parse(actualUserId));
                    if (response != null && response.IsSuccessStatusCode)
                    {
                        return "User deleted successfully";
                    }
                    else
                    {
                        return response != null ? response.BOSErrors[0].Message : "We are unable to delete this user at this time. Please try again.";
                    }
                }
                else
                {
                    return "UserId cannot be null. Please check and try again.";
                }

            }
            catch (Exception ex)
            {
                Logger.LogException("Users", "DeleteUser", ex);
                return ex.Message;
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description:  Is triggered when the 'Update' button is clicked with the updated details of the selected user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> UpdateUserInfo([FromBody]User user)
        {
            try
            {
                if (user != null)
                {
                    StringConversion stringConversion = new StringConversion();
                    user.Id = Guid.Parse(stringConversion.DecryptString(user.UpdatedId));
                    var extendUserResponse = await _bosAuthClient.ExtendUserAsync(user);
                    if (extendUserResponse != null && extendUserResponse.IsSuccessStatusCode)
                    {
                        return "User's information updated successfully";
                    }
                    else
                    {
                        return extendUserResponse != null ? extendUserResponse.BOSErrors[0].Message : "We are unable to update this user's information at this time. Please try again.";
                    }
                }
                else
                {
                    return "User data inputted is inaccurate. Please try again";
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Users", "UpdateUserInfo", ex);
                return ex.Message;
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Private method to fetch the data necessary to render the page
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
                    currentModuleId = moduleOperations.Where(i => i.Code == "USERS").Select(i => i.Id).ToList()[0];
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
                }

                StringConversion stringConversion = new StringConversion();
                var userList = await _bosAuthClient.GetUsersWithRolesAsync<User>();
                if (userList != null && userList.IsSuccessStatusCode)
                {
                    var updatedUserList = userList.Users.Select(c => { c.UpdatedId = stringConversion.EncryptString(c.Id.ToString()); return c; }).ToList();
                    model.UserList = updatedUserList;
                }
                return model;
            }

            catch (Exception ex)
            {
                Logger.LogException("Users", "GetPageData", ex);
                return null;
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Private method to generate password
        /// </summary>
        /// <returns></returns>
        private string CreatePassword()
        {
            PasswordOptions opts = new PasswordOptions()
            {
                RequiredLength = 10,
                RequiredUniqueChars = 4,
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true
            };

            string[] randomChars = new[] {
                "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
                "abcdefghijkmnopqrstuvwxyz",    // lowercase
                "0123456789",                   // digits
                "!@$?_-"                        // non-alphanumeric
            };
            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (opts.RequireUppercase)
            {
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);
            }

            if (opts.RequireLowercase)
            {
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);
            }

            if (opts.RequireDigit)
            {
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);
            }

            if (opts.RequireNonAlphanumeric)
            {
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);
            }

            for (int i = chars.Count; i < opts.RequiredLength
                || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }
    }
}

