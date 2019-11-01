using System;
using System.ComponentModel.DataAnnotations;

namespace BOS.StarterCode.Models
{
    public class AuthModel
    {
        [Required(ErrorMessage = "Required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Required")]
        public string Password { get; set; }

        public AuthModel()
        {
        }

        public AuthModel(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("Username cannot be null or empty");
            }
        }

        public AuthModel(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("Username and password cannot be null or empty");
            }
        }
    }
}
