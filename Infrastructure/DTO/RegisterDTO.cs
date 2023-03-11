using System.ComponentModel.DataAnnotations;

namespace Infrastructure
{
    public class RegisterDTO
    {
        [Required]
        public required string FirstName { get; set; }
        [Required]
        public required string LastName { get; set; }
        [Required]
        public required string Email { get; set; }
        [Required]
        public required string Password { get; set; }
        [Required]
        [Compare("Password",ErrorMessage ="Password and Repeat password should match")]
        public required string RepeatPassword { get; set; }
    }
    public class UserEditSubmitDTO
    {
        [Required]
        public required string UserId { get; set; }
        [Required]
        public required string FirstName { get; set; }
        [Required]
        public required string LastName { get; set; }
        [Required]
        public required string Email { get; set; }
    }
    public class UserSubmitDTO
    {
        [Required]
        public required string FirstName { get; set; }
        [Required]
        public required string LastName { get; set; }
        [Required]
        public required string Email { get; set; }
    }
}
