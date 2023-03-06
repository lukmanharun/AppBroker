using System.ComponentModel.DataAnnotations;

namespace AppBroker.BusinessCore.Entity.DTO
{
    public class SignInDTO
    {
        [Required(AllowEmptyStrings = false)]
        public string Email { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string Password { get; set; }
    }
}
