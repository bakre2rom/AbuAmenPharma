using System.ComponentModel.DataAnnotations;

namespace AbuAmenPharma.ViewModels
{
    public class AdminUserCreateVM
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = "";

        [Required]
        [Compare(nameof(Password), ErrorMessage = "كلمتا المرور غير متطابقتين.")]
        public string ConfirmPassword { get; set; } = "";

        [Required]
        public string Role { get; set; } = "Operator";
    }
}
