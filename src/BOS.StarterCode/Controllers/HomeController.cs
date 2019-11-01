using BOS.StarterCode.Helpers;
using BOS.StarterCode.Models;
using BOS.StarterCode.Models.BOSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;

namespace BOS.StarterCode.Controllers
{
    [Authorize(Policy = "IsAuthenticated")]
    public class HomeController : Controller
    {
        private Logger Logger;

        public HomeController()
        {
            Logger = new Logger();
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Returns the "Dashboard" view in the application with appropriate data
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            try
            {
                return View(GetPageData());
            }
            catch (Exception ex)
            {
                Logger.LogException("Home", "Index", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Is triggered when a "Custom Module" is clicked in the Navigation menu.  
        /// </summary>
        /// <param name="selectedModuleId"></param>
        /// <returns></returns>
        public IActionResult NavigationMenu(string selectedModuleId)
        {
            try
            {
                var model = GetPageData();
                if (model != null)
                {
                    model.CurrentModuleId = selectedModuleId;
                }
                return View("Index", model);
            }
            catch (Exception ex)
            {
                Logger.LogException("Home", "NavigationMenu", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Returns an Error view if something goes wrong
        /// </summary>
        /// <returns></returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Is a private method that fetches all the necessary information to render on the View
        /// </summary>
        /// <returns></returns>
        private dynamic GetPageData()
        {
            try
            {
                var modules = HttpContext.Session.GetObject<List<Module>>("Modules");
                dynamic model = new ExpandoObject();
                model.Modules = modules;
                model.Username = User.FindFirst(c => c.Type == "Username").Value.ToString();
                model.Initials = User.FindFirst(c => c.Type == "Initials").Value.ToString();
                model.Roles = User.FindFirst(c => c.Type == "Role").Value.ToString();
                model.ModuleOperations = HttpContext.Session.GetObject<List<Module>>("ModuleOperations");
                model.CurrentModuleId = null;
                return model;

            }
            catch (Exception ex)
            {
                Logger.LogException("Home", "NavigationMenu", ex);
                return null;
            }
        }
    }
}
