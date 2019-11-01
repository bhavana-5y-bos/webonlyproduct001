using BOS.StarterCode.Helpers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Dynamic;

namespace BOS.StarterCode.Controllers
{
    public class NavigationController : Controller
    {
        private Logger Logger;

        public NavigationController()
        {
            Logger = new Logger();
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Is triggered when the Navigation Menu options are clicked. Uses the selected modules 'code' to identify and navigate to the respective controller
        /// </summary>
        /// <param name="id"></param>
        /// <param name="code"></param>
        /// <param name="isDefault"></param>
        /// <returns></returns>
        public IActionResult NavigateToModule(Guid id, string code, bool isDefault)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(code))
                {
                    //if (isDefault)
                    //{
                    switch (code)
                    {
                        case "MYPFL":
                            return RedirectToAction("Index", "Profile");
                        case "USERS":
                            return RedirectToAction("Index", "Users");
                        case "ROLES":
                            return RedirectToAction("Index", "Roles");
                        case "PRMNS":
                            return RedirectToAction("Index", "Permissions");
                        default:
                            return RedirectToAction("NavigationMenu", "Home", new { selectedModuleId = id });
                    }
                    //}
                    //else
                    //{
                    //    ViewBag.ModuleSelected = "You've selected a custom module. Implement the logic to display the correct view";
                    //    return RedirectToAction("NavigationMenu", "Home", new { selectedModuleId = id });
                    //}
                }
                else
                {
                    return RedirectToAction("NavigationMenu", "Home", new { selectedModuleId = "" });
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Navigation", "NavigateToModule", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }
    }
}