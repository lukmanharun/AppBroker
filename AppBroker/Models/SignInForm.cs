using System.ComponentModel.DataAnnotations;

namespace AppBroker.Models
{
    public class SignInForm
    {
        [Required(AllowEmptyStrings = false)]
        public string Email { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string Password { get; set; }
    }
}
