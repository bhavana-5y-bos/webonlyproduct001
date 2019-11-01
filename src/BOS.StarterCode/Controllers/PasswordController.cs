using BOS.Auth.Client;
using BOS.StarterCode.Helpers;
using BOS.StarterCode.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Dynamic;
using System.Threading.Tasks;

namespace BOS.StarterCode.Controllers
{
    public class PasswordController : Controller
    {
        private readonly IAuthClient _bosAuthClient;

        private Logger Logger;

        public PasswordController(IAuthClient authClient)
        {
            _bosAuthClient = authClient;
            Logger = new Logger();
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Sets the password of the user, either after registration or forgot password operation is performed. 
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<IActionResult> ResetPassword(ChangePassword password)
        {
            try
            {
                if (password != null)
                {
                    string userId = password.CurrentPassword;

                    var response = await _bosAuthClient.ForcePasswordChangeAsync(Guid.Parse(password.CurrentPassword), password.NewPassword);
                    if (response != null && response.IsSuccessStatusCode)
                    {
                        ViewBag.SuccessMessage = "Password reset successfully";
                        return View();
                    }
                    else
                    {
                        dynamic model = new ExpandoObject();
                        model.Message = response != null ? response.BOSErrors[0].Message : "Unable to update the password at this time. Please try again later.";
                        model.StackTrace = "The password object that was provided was null";
                        return View("ErrorPage", model);
                    }
                }
                else
                {
                    dynamic model = new ExpandoObject();
                    model.Message = "Passwords cannot be null";
                    model.StackTrace = "The password object that was provided was null";
                    return View("ErrorPage", model);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Password", "ResetPassword", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Navigates the user back to the Login Screen
        /// </summary>
        /// <returns></returns>
        public IActionResult GotBackToLogin()
        {
            return RedirectToAction("Index", "Auth");
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Is triggered when the user gets to the Verification Link. If the slug is valid it shows the view to set/ reset the password, else just shows a message
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Reset(string slug)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(slug))
                {
                    var result = await _bosAuthClient.VerifySlugAsync(slug);
                    
                    if (result != null && result.IsSuccessStatusCode)
                    {
                        ViewBag.UserId = result.UserId;
                    }
                    else
                    {
                        ViewBag.Message = "The link has either expired or is invalid. If you have just registered, then get in touch with your admistrator for a new password. If you have forgotten your password, retry again in some time.";
                    }

                    return View("ResetPassword");
                }
                else
                {
                    dynamic model = new ExpandoObject();
                    model.Message = "The slug string cannot be empty or null.";
                    model.StackTrace = "";
                    return View("ErrorPage", model);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Password", "Reset", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }
    }
}