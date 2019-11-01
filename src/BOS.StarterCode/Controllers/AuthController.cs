using BOS.Auth.Client;
using BOS.Auth.Client.ClientModels;
using BOS.Email.Client;
using BOS.Email.Client.ClientModels;
using BOS.IA.Client;
using BOS.StarterCode.Helpers;
using BOS.StarterCode.Models;
using BOS.StarterCode.Models.BOSModels;
using BOS.StarterCode.Models.BOSModels.Permissions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BOS.StarterCode.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthClient _bosAuthClient;
        private readonly IIAClient _bosIAClient;
        private readonly IEmailClient _bosEmailClient;
        private readonly IConfiguration _configuration;

        private Logger Logger;

        public AuthController(IAuthClient authClient, IIAClient iaClient, IEmailClient emailClient, IConfiguration configuration)
        {
            _bosAuthClient = authClient;
            _bosIAClient = iaClient;
            _bosEmailClient = emailClient;
            _configuration = configuration;
            Logger = new Logger();
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Returns the "Login" view - The landing page of the application
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            try
            {
                //Check if user is authenticated then redirect him to clients
                if (User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Auth", "Index", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }

            return View("Index");
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggers when the Login button is clicked
        /// </summary>
        /// <param name="authObj"></param>
        /// <returns></returns>
        public async Task<ActionResult> AuthenticateUser(AuthModel authObj)
        {
            try
            {
                if (!HttpContext.Request.Cookies.ContainsKey(".AspNet.Consent"))
                {
                    ModelState.AddModelError("CustomError", "Before proceeding, please 'Accept' our Cookies' terms.");
                    return View("Index", new AuthModel());
                }
                if (authObj != null)
                {
                    var result = await _bosAuthClient.SignInAsync(authObj.Username.Trim(), authObj.Password);
                    if (result.IsVerified)
                    {

                        var userRoles = await _bosAuthClient.GetUserByIdWithRolesAsync<User>(result.UserId.Value);
                        var user = userRoles.User;
                        var roles = user.Roles;

                        // Convert Roles Array into a comma separated string containing roles
                        string rolesString = string.Empty;
                        if (roles != null && roles.Count > 0)
                        {
                            foreach (UserRole userRole in roles)
                            {
                                RoleUser role = userRole.Role;
                                rolesString = (!string.IsNullOrEmpty(rolesString)) ? (rolesString + "," + role.Name) : (role.Name);
                            }
                        }

                        //Create Claim Identity.
                        var claims = new List<Claim>{ new Claim("CreatedOn", DateTime.UtcNow.ToString()),
                                              new Claim("Email", user.Email),
                                              new Claim("Initials", user.FirstName[0].ToString() + user.LastName[0].ToString()),
                                              new Claim("Name", user.FirstName +" " + user.LastName),
                                              new Claim("Role", rolesString),
                                              new Claim("UserId", user.Id.ToString()),
                                              new Claim("Username", user.Username.ToString()),
                                              new Claim("IsAuthenticated", "True")
                                            };
                        var userIdentity = new ClaimsIdentity(claims, "Auth");
                        ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);

                        //Sign In created claims Identity Principal with cookie Authentication scheme
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
                        {
                            ExpiresUtc = DateTime.UtcNow.AddMinutes(3000),
                            IsPersistent = false,
                            AllowRefresh = false
                        });

                        //Getting the permissions
                        var permissionSet = await _bosIAClient.GetOwnerPermissionsSetsAsync<Permissions>(result.UserId.Value);
                        if (permissionSet.IsSuccessStatusCode)
                        {
                            //Set Permissions in Sessions
                            HttpContext.Session.SetObject("ModuleOperations", permissionSet.Permissions.Modules);
                        }

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("CustomError", "Username or password is incorrect");
                        return View("Index", new AuthModel());
                    }
                }
                else
                {
                    return View("Index", new AuthModel());
                }
            }
            catch (ArgumentNullException ex)
            {
                Logger.LogException("Auth", "AuthenticateUser", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
            catch (Exception ex)
            {
                Logger.LogException("Auth", "AuthenticateUser", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: When clicked on the 'Register' link on the page, navigates to the 'Register view
        /// </summary>
        /// <returns></returns>
        public ActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggers when the Register button is clicked
        /// </summary>
        /// <param name="registerObj"></param>
        /// <returns></returns>
        public async Task<ActionResult> RegisterUser(RegistrationModel registerObj)
        {
            try
            {
                registerObj.EmailAddress = registerObj.EmailAddress.Trim();
                registerObj.FirstName = registerObj.FirstName.Trim();
                registerObj.LastName = registerObj.LastName.Trim();

                var result = await _bosAuthClient.AddNewUserAsync<BOSUser>(registerObj.EmailAddress, registerObj.EmailAddress, CreatePassword());
                if (result != null)
                {
                    if (result.IsSuccessStatusCode)
                    {
                        User user = new User
                        {
                            Id = result.User.Id,
                            CreatedOn = DateTime.UtcNow,
                            Deleted = false,
                            Email = registerObj.EmailAddress,
                            FirstName = registerObj.FirstName,
                            LastModifiedOn = DateTime.UtcNow,
                            LastName = registerObj.LastName,
                            Username = registerObj.EmailAddress
                        };

                        var extendUserResponse = await _bosAuthClient.ExtendUserAsync(user);
                        if (extendUserResponse.IsSuccessStatusCode)
                        {
                            List<Role> roleList = new List<Role>();

                            var availableRoles = await _bosAuthClient.GetRolesAsync<Role>();
                            if (availableRoles.IsSuccessStatusCode)
                            {
                                Role defaultRole = availableRoles.Roles.FirstOrDefault(i => i.Name == "User");
                                roleList.Add(defaultRole);
                                var roleResponse = await _bosAuthClient.AssociateUserToMultipleRolesAsync(result.User.Id, roleList);
                                if (roleResponse.IsSuccessStatusCode)
                                {
                                    var slugResponse = await _bosAuthClient.CreateSlugAsync(registerObj.EmailAddress);
                                    if (slugResponse.IsSuccessStatusCode)
                                    {
                                        var slug = slugResponse.Slug;

                                        ViewBag.Message = "Welcome! You've been successfully registered with us. Check you inbox for an activation link.";

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
                                                    Email = registerObj.EmailAddress,
                                                    Name = registerObj.FirstName + " " + registerObj.LastName
                                                }
                                            }
                                        };
                                        var templateResponse = await _bosEmailClient.GetTemplateAsync<Template>();
                                        if (templateResponse.IsSuccessStatusCode)
                                        {
                                            emailObj.TemplateId = templateResponse.Templates.Where(i => i.Name == "UserRegistration").Select(i => i.Id).ToList()[0];
                                        }
                                        else
                                        {
                                            ModelState.AddModelError("CustomError", "Sorry! We could not send you an email. Please try again later");
                                            return View("Index");
                                        }

                                        var spResponse = await _bosEmailClient.GetServiceProviderAsync<ServiceProvider>();
                                        if (spResponse.IsSuccessStatusCode)
                                        {
                                            emailObj.ServiceProviderId = spResponse.ServiceProvider[0].Id;
                                        }
                                        else
                                        {
                                            ModelState.AddModelError("CustomError", "Sorry! We could not send you an email. Please try again later");
                                            return View("Index");
                                        }

                                        emailObj.Substitutions = new List<Substitution>();
                                        emailObj.Substitutions.Add(new Substitution { Key = "companyUrl", Value = _configuration["PublicUrl"] });
                                        emailObj.Substitutions.Add(new Substitution { Key = "companyLogo", Value = _configuration["PublicUrl"] + "/wwwroot/images/logo.png" });
                                        emailObj.Substitutions.Add(new Substitution { Key = "usersName", Value = registerObj.FirstName + " " + registerObj.LastName });
                                        emailObj.Substitutions.Add(new Substitution { Key = "applicationName", Value = _configuration["ApplicationName"] });
                                        emailObj.Substitutions.Add(new Substitution { Key = "activationUrl", Value = _configuration["PublicUrl"] + "/Password/Reset?slug=" + slug.Value });
                                        emailObj.Substitutions.Add(new Substitution { Key = "thanksCredits", Value = "Team StarterCode" });

                                        var emailResponse = await _bosEmailClient.SendEmailAsync<IEmail>(emailObj);
                                        if (!emailResponse.IsSuccessStatusCode)
                                        {
                                            ModelState.AddModelError("CustomError", emailResponse.BOSErrors[0].Message);
                                        }
                                        return View("Index");
                                    }
                                }
                            }
                        }
                        ModelState.AddModelError("CustomError", result.BOSErrors[0].Message);
                        return View("Register");
                    }
                    else
                    {
                        ModelState.AddModelError("CustomError", result.BOSErrors[0].Message);
                        return View("Register");
                    }
                }
                else
                {
                    ModelState.AddModelError("CustomError", "Something went wrong. We are currently unable to register you. Please try again later.");
                    return View("Register");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Auth", "RegisterUser", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: When clicked on the 'Register' link on the page, navigates to the 'Register view
        /// </summary>
        /// <returns></returns>
        public ActionResult ForgotPassword()
        {
            return View();
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggers when the Register button is clicked
        /// </summary>
        /// <param name="forgotPasswordObj"></param>
        /// <returns></returns>
        public async Task<ActionResult> ForgotPasswordAction(ForgotPassword forgotPasswordObj)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string emailAddress = forgotPasswordObj.EmailAddress.Trim();

                    var userResponse = await _bosAuthClient.GetUserByEmailAsync<BOSUser>(emailAddress);

                    if (userResponse != null && userResponse.Users != null)
                    {
                        var slugResponse = await _bosAuthClient.CreateSlugAsync(emailAddress);
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
                                            Email = emailAddress,
                                            Name = ""
                                        }
                                    }
                            };
                            var templateResponse = await _bosEmailClient.GetTemplateAsync<Template>();
                            if (templateResponse != null && templateResponse.IsSuccessStatusCode)
                            {
                                emailObj.TemplateId = templateResponse.Templates.Where(i => i.Name == "ForgotPassword").Select(i => i.Id).ToList()[0];
                            }
                            else
                            {
                                ModelState.AddModelError("CustomError", "Sorry! We could not send you an email. Please try again later");
                                return View("Index");
                            }

                            var spResponse = await _bosEmailClient.GetServiceProviderAsync<ServiceProvider>();
                            if (spResponse != null && spResponse.IsSuccessStatusCode)
                            {
                                emailObj.ServiceProviderId = spResponse.ServiceProvider[0].Id;
                            }
                            else
                            {
                                ModelState.AddModelError("CustomError", "Sorry! We could not send you an email. Please try again later");
                                return View("Index");
                            }

                            emailObj.Substitutions = new List<Substitution>();
                            emailObj.Substitutions.Add(new Substitution { Key = "companyUrl", Value = _configuration["PublicUrl"] });
                            emailObj.Substitutions.Add(new Substitution { Key = "companyLogo", Value = _configuration["PublicUrl"] + "/wwwroot/images/logo.png" });
                            emailObj.Substitutions.Add(new Substitution { Key = "usersName", Value = "" });
                            emailObj.Substitutions.Add(new Substitution { Key = "applicationName", Value = _configuration["ApplicationName"] });
                            emailObj.Substitutions.Add(new Substitution { Key = "resetUrl", Value = _configuration["PublicUrl"] + "/Password/Reset?slug=" + slug.Value });
                            emailObj.Substitutions.Add(new Substitution { Key = "thanksCredits", Value = "Team StarterCode" });

                            var emailResponse = await _bosEmailClient.SendEmailAsync<IEmail>(emailObj);
                            if (!emailResponse.IsSuccessStatusCode)
                            {
                                ModelState.AddModelError("CustomError", emailResponse.BOSErrors[0].Message);
                                return View("Index");
                            }
                        }
                    }
                }
                ViewBag.Message = "Check your inbox for an email with a link to reset your password.";
                return View("Index");
            }
            catch (Exception ex)
            {
                Logger.LogException("Auth", "ForgotPasswordAction", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggers after the user has inputted the email address on the View to recieve the email with a link to reset the password
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> ForcePasswordChange([FromBody]JObject data)
        {
            try
            {
                StringConversion stringConversion = new StringConversion();
                string userId = stringConversion.DecryptString(data["userId"].ToString());

                string password = data["password"].ToString();
                var response = await _bosAuthClient.ForcePasswordChangeAsync(Guid.Parse(userId), password);
                if (response != null && response.IsSuccessStatusCode)
                {
                    return "Password updated successfully";
                }
                else
                {
                    Logger.LogException("Auth", "ForcePasswordChange", null);
                    return "Something went wrong. We are not able to change the password at this moment. Please try again later.";
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Auth", "ForcePasswordChange", ex);
                return "Something went wrong. We are not able to change the password at this moment. Please try again later.";
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Logout
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return GoToReturnUrl("/Auth");
            }
            catch (Exception ex)
            {
                Logger.LogException("Auth", "Logout", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: To navigate the user back to the Login Page.
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        private IActionResult GoToReturnUrl(string returnUrl)
        {
            try
            {
                if (Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Auth", "GoToReturnUrl", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: A private method that randomly generates Password string during User Registration
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
