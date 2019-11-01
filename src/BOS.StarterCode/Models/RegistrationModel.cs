using System;
using System.ComponentModel.DataAnnotations;

namespace BOS.StarterCode.Models
{
    public class RegistrationModel
    {
        [Required(ErrorMessage = "Required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Required")]
        [EmailAddress(ErrorMessage = "Incorrect email format")]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        public RegistrationModel()
        {

        }

        public RegistrationModel(string firstName, string lastName, string email, string password)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("None of the fields can be null or empty");
            }
        }
    }
}
