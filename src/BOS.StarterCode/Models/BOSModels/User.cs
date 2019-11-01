using BOS.Auth.Client.ClientModels;
using System;
using System.Collections.Generic;

namespace BOS.StarterCode.Models.BOSModels
{
    public class User : IUser
    {

        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool Deleted { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset LastModifiedOn { get; set; }
        public List<UserRole> Roles { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UpdatedId { get; set; }

        public User()
        {
            Roles = new List<UserRole>();
        }

        internal static object FindFirst(Func<object, bool> p)
        {
            throw new NotImplementedException();
        }
    }
}
