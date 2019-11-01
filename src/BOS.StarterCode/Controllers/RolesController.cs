using BOS.Auth.Client;
using BOS.StarterCode.Helpers;
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
    public class RolesController : Controller
    {
        private readonly IAuthClient _bosAuthClient;

        private Logger Logger;

        public RolesController(IAuthClient authClient)
        {
            _bosAuthClient = authClient;

            Logger = new Logger();
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Lists all the roles available in the application
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            return View(await GetPageData());
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggers when the "Add Role" button is clicked
        /// </summary>
        /// <returns></returns>
        public IActionResult AddNewRole()
        {
            return View("AddRole");
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggers when the data for the new role is entered to be saved into the database
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task<ActionResult> AddRole(Role role)
        {
            try
            {
                if (role != null)
                {
                    var response = await _bosAuthClient.AddRoleAsync<Role>(role);
                    if (response != null && response.IsSuccessStatusCode)
                    {
                        ViewBag.Message = "Role added successfully";
                        return View("Index", await GetPageData());
                    }
                    else
                    {
                        ModelState.AddModelError("CustomError", response.BOSErrors[0].Message);
                        return View("AddRole", role);
                    }
                }
                else
                {
                    ModelState.AddModelError("CustomError", "The data inputted for role is inaccurate. Please try again.");
                    return View("AddRole", role);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Roles", "AddRole", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggered when the "Edit Role" button is clicked
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public async Task<IActionResult> EditRole(string roleId)
        {
            try
            {
                if (!string.IsNullOrEmpty(roleId))
                {
                    Role role = new Role();
                    var roleInfo = await _bosAuthClient.GetRoleByIdAsync<Role>(Guid.Parse(roleId));
                    if (roleInfo != null && roleInfo.IsSuccessStatusCode)
                    {
                        role = roleInfo.Role;
                    }
                    return View("EditRole", role);
                }
                else
                {
                    ModelState.AddModelError("CustomError", "The selected role has incorrect Id. Please try again");
                    return View("Index", await GetPageData());
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Roles", "EditRole", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggers when the updated data for new role is entered to be saved into the database
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task<IActionResult> UpdateRole(Role role)
        {
            try
            {
                if (role != null)
                {
                    var response = await _bosAuthClient.UpdateRoleAsync<Role>(role.Id, role);
                    if (response != null && response.IsSuccessStatusCode)
                    {
                        ViewBag.Message = "Role updated successfully";
                        return View("Index", await GetPageData());
                    }
                    else
                    {
                        ModelState.AddModelError("CustomError", response.BOSErrors[0].Message);
                        return View("EditRole", role);
                    }
                }
                else
                {
                    ModelState.AddModelError("CustomError", "The inputted data was inaccurate. Please try again.");
                    return View("EditRole", role);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Roles", "UpdateRole", ex);

                dynamic model = new ExpandoObject();
                model.Message = ex.Message;
                model.StackTrace = ex.StackTrace;
                return View("ErrorPage", model);
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggered when the "Manage Permissions" button is clicked. This naviogates the user to the Permissions Controller
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public ActionResult RoleManagePermissions(string roleId, string roleName)
        {
            return RedirectToAction("FetchPermissions", "Permissions", new { roleId, roleName });
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggered from the Users Controller when a user's role is being updated
        /// </summary>
        /// <param name="updatedRoles"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> UpdateUserRoles([FromBody]List<Role> updatedRoles)
        {
            try
            {
                if (updatedRoles.Count > 0)
                {
                    Guid userId = Guid.Parse(User.FindFirst(c => c.Type == "UserId").Value.ToString());

                    var response = await _bosAuthClient.AssociateUserToMultipleRolesAsync(userId, updatedRoles);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception("Something went wrong while updating the roles. Please try again later");
                    }
                    else
                    {
                        return "User roles updates successfully";
                    }
                }
                else
                {
                    return "Roles to associate with the user cannot be empty";
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Roles", "UpdateRole", ex);
                return ex.Message;
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggered from the Users Controller when a user's role is being updated by the Admin 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> UpdateUserRolesByAdmin([FromBody]JObject data)
        {
            try
            {
                List<Role> updatedRoles = data["UpdatedRoles"].ToObject<List<Role>>();
                var updatedUserId = data["UserId"].ToString();
                StringConversion stringConversion = new StringConversion();
                Guid userId = Guid.Parse(stringConversion.DecryptString(updatedUserId));
                if (updatedRoles.Count > 0)
                {
                    var response = await _bosAuthClient.AssociateUserToMultipleRolesAsync(userId, updatedRoles);
                    if (response != null && !response.IsSuccessStatusCode)
                    {
                        throw new Exception("Something went wrong while updating the roles. Please try again later");
                    }
                    else
                    {
                        return "User's roles updates successfully";
                    }
                }
                else
                {
                    return "Roles to associate with the user cannot be empty";
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Roles", "UpdateRole", ex);
                return ex.Message;
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Triggers when a role is seleted to be deleted, after confirmation on the UI
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> DeleteRole([FromBody]string roleId)
        {
            try
            {
                if (!string.IsNullOrEmpty(roleId))
                {
                    var response = await _bosAuthClient.DeleteRoleAsync(Guid.Parse(roleId));
                    if (response.IsSuccessStatusCode)
                    {
                        return "Role deleted successfully";

                    }
                    else
                    {
                        return response.BOSErrors[0].Message;
                    }
                }
                else
                {
                    return "The selected roles has an inaccurate Id. Please check and try again.";
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Roles", "DeleteRole", ex);
                return ex.Message;
            }
        }

        /// <summary>
        /// Author: BOS Framework, Inc
        /// Description: Fetches data necessary for rendering on the page
        /// </summary>
        /// <returns></returns>
        private async Task<dynamic> GetPageData()
        {
            try
            {
                var moduleOperations = HttpContext.Session.GetObject<List<Module>>("ModuleOperations");
                List<Guid> currentModuleIds = new List<Guid>();
                try
                {
                    currentModuleIds = moduleOperations.Where(i => i.Code == "ROLES" || i.Code == "PRMNS").Select(i => i.Id).ToList();
                }
                catch (ArgumentNullException)
                {
                    currentModuleIds = null;
                }

                var currentOperations = moduleOperations.Where(i => currentModuleIds.Contains(i.Id)).Select(i => i.Operations).ToList()[0];
                var currentOperations1 = moduleOperations.Where(i => currentModuleIds.Contains(i.Id)).Select(i => i.Operations).ToList()[1];
                string operationsString = String.Join(",", currentOperations.Select(i => i.Code));
                operationsString = operationsString + "," + String.Join(",", currentOperations1.Select(i => i.Code));

                dynamic model = new ExpandoObject();
                model.ModuleOperations = HttpContext.Session.GetObject<List<Module>>("ModuleOperations");
                model.Operations = operationsString;
                model.CurrentModuleId = currentModuleIds[0];
                model.Initials = User.FindFirst(c => c.Type == "Initials").Value.ToString();

                if (User.FindFirst(c => c.Type == "Username") != null || User.FindFirst(c => c.Type == "Role") != null)
                {
                    model.Username = User.FindFirst(c => c.Type == "Username").Value.ToString();
                    model.Roles = User.FindFirst(c => c.Type == "Role").Value.ToString();
                }

                var response = await _bosAuthClient.GetRolesAsync<Role>();
                if (response.IsSuccessStatusCode)
                {
                    model.RoleList = response.Roles;
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