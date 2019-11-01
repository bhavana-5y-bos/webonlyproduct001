using System.ComponentModel.DataAnnotations;

namespace BOS.StarterCode.Models
{
    public class ForgotPassword
    {
        [Required(ErrorMessage = "Required")]
        [EmailAddress]
        public string EmailAddress { get; set; }
    }
}
