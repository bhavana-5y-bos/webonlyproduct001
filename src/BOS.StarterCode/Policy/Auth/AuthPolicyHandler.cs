using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BOS.StarterCode.Policy.Auth
{
    public class AdminOnlyRequirement : IAuthorizationRequirement
    {
        public AdminOnlyRequirement(string[] roles)
        {
            allowedRoles = roles;
        }
        public string[] allowedRoles { get; set; }
    }
    public class IsAuthenticatedRequirement : IAuthorizationRequirement
    {
        public IsAuthenticatedRequirement()
        {
            IsAuthenticated = true;
        }
        public bool IsAuthenticated { get; set; }
    }
    //Check if user is authenticated 
    public class IsAuthenticatedHandler : AuthorizationHandler<IsAuthenticatedRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsAuthenticatedRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == "IsAuthenticated") || !context.User.HasClaim(c => c.Type == "UserId")) //
            {
                return Task.CompletedTask;
            }
            //Check if is Authenticated
            if (requirement.IsAuthenticated.ToString().Equals(context.User.FindFirst(c => c.Type == "IsAuthenticated").Value))
            {
                //Check for User ID 
                try
                {
                    Guid.Parse((context.User.FindFirst(c => c.Type == "UserId").Value).ToString());
                }
                catch (Exception)
                {
                    return Task.CompletedTask;
                }
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
    //Check if user in Role() has access to the controller
    public class AdminOnlyHandler : AuthorizationHandler<AdminOnlyRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminOnlyRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == "Role"))
            {
                return Task.CompletedTask;
            }
            //Check if is Authenticated
            if (requirement.allowedRoles.Contains(Convert.ToString(context.User.FindFirst(c => c.Type == "Role").Value)))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
