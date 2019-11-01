using BOS.IA.Client;
using BOS.IA.Client.ClientModels;
using BOS.StarterCode.Helpers;
using BOS.StarterCode.Models.BOSModels;
using BOS.StarterCode.Models.BOSModels.Permissions;
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
    public class PermissionsController : Controller
    {
        private readonly IIAClient _bosIAClient;

        private Logger Logger;

        public PermissionsController(IIAClient iaClient)
        {
            _bosIAClient = iaClient;
            Logger = new Logger();
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Returns the view that displays all the modules and operations that are associated with the given role
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public async Task<ActionResult> FetchPermissions(string roleId, string roleName)
        {
            try
            {
                var model = GetPageData();
                if (model == null)
                    model = new ExpandoObject();

                if (!string.IsNullOrWhiteSpace(roleId))
                {
                    var ownerPermissionsresponse = await _bosIAClient.GetOwnerPermissionsSetsAsFlatAsync<PermissionsModule>(Guid.Parse(roleId));

                    List<Module> allModules = new List<Module>();
                    List<Operation> allOperations = new List<Operation>();
                    List<IPermissionsOperation> permittedOperations = new List<IPermissionsOperation>();
                    List<IPermissionsSet> permittedModules = new List<IPermissionsSet>();

                    if (ownerPermissionsresponse != null && ownerPermissionsresponse.IsSuccessStatusCode)
                    {
                        permittedModules = ownerPermissionsresponse.Permissions.Modules;
                        permittedOperations = ownerPermissionsresponse.Permissions.Operations;
                    }

                    var modulesResponse = await _bosIAClient.GetModulesAsync<Module>(true, true);
                    if (modulesResponse != null && modulesResponse.IsSuccessStatusCode)
                    {
                        allModules = modulesResponse.Modules;
                    }

                    var operationsResponse = await _bosIAClient.GetOperationsAsync<Operation>(true, true);
                    if (operationsResponse != null && operationsResponse.IsSuccessStatusCode)
                    {
                        allOperations = operationsResponse.Operations;
                    }

                    foreach (PermissionsSet module in permittedModules)
                    {
                        var moduleObj = allModules.FirstOrDefault(x => x.Id == module.ModuleId);

                        if (moduleObj != null)
                        {
                            moduleObj.IsPermitted = true;
                            if (moduleObj.Operations.Count > 0)
                            {
                                foreach (Operation operation in moduleObj.Operations)
                                {
                                    var operationObj = permittedOperations.FirstOrDefault(x => x.OperationId == operation.Id);
                                    if (operationObj != null)
                                    {
                                        operation.IsPermitted = true;
                                    }
                                }
                            }
                        }
                    }

                    model.ModuleOperations = allModules;
                    model.OwnerId = roleId;
                    model.RoleName = roleName;
                    return View("Index", model);
                }
                else
                {
                    ModelState.AddModelError("CustomError", "The selected role does not have a verified Id. Please try again.");
                    return View("Index", model);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Permissions", "FetchPermissions", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Navigates the user back to the Role view
        /// </summary>
        /// <returns></returns>
        public IActionResult BackToRoles()
        {
            return RedirectToAction("Index", "Roles");
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Saves all the changes made to the permissions for the given role
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> UpdatePermissions([FromBody] JObject data)
        {
            try
            {
                PermissionsModule permissionsModule = new PermissionsModule();
                List<PermissionsSet> modules = data["Modules"].ToObject<List<PermissionsSet>>();
                permissionsModule.Modules = new List<IPermissionsSet>();
                permissionsModule.Modules.AddRange(modules);

                List<PermissionsOperation> operations = data["Operations"].ToObject<List<PermissionsOperation>>();
                permissionsModule.Operations = new List<IPermissionsOperation>();
                permissionsModule.Operations.AddRange(operations);

                permissionsModule.OwnerId = Guid.Parse(data["OwnerId"].ToString());
                permissionsModule.Type = SetType.Role;

                var response = await _bosIAClient.AddPermissionsAsync<PermissionsModule>(permissionsModule);
                if (response != null && response.IsSuccessStatusCode)
                {
                    return "Permissions updated successfully";
                }
                else
                {
                    return response.BOSErrors[0].Message;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Permissions", "FetchPermissions", ex);
                return ex.Message;
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Fetches the data necessary for rendering on the page
        /// </summary>
        /// <returns></returns>
        private dynamic GetPageData()
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

                dynamic model = new ExpandoObject();
                model.ModuleOperations = moduleOperations;
                model.CurrentModuleId = currentModuleId;
                model.Initials = User.FindFirst(c => c.Type == "Initials").Value.ToString();

                if (User.FindFirst(c => c.Type == "Username") != null || User.FindFirst(c => c.Type == "Role") != null)
                {
                    model.Username = User.FindFirst(c => c.Type == "Username").Value.ToString();
                    model.Roles = User.FindFirst(c => c.Type == "Role").Value.ToString();
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