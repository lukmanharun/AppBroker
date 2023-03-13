using System.ComponentModel.DataAnnotations;

namespace Infrastructure
{
    public class SignInDTO
    {
        [Required(AllowEmptyStrings = false)]
        public required string Email { get; set; }
        [Required(AllowEmptyStrings = false)]
        public required string Password { get; set; }
        public required bool IsRememberme { get; set; }
    }
}
