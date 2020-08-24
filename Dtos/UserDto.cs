using System.ComponentModel.DataAnnotations;

namespace core_api.Models
{
    public class UserProfileDto : UserDto
    {
        public string Token { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhotoUrl { get; set; }
        public bool EmailConfirmed { get; set; }
        public string NationalId { get; set; }
        public string PhoneNumber { get; set; }

        public bool Verified { get; set; }
    }

    public class UpdateUserRequest
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string NationalId { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
    }

    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string NationalId { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class InviteUserRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string NationalId { get; set; }
        [Required]
        public string PhoneNumber { get; set; }

        public string Role { get; set; } = Models.Role.None;
    }

    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class ForgetPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class ConfirmEmailRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Token { get; set; }
    }

    public class ResetPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
        [Required]
        public string Token { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }

    public class FacebookAuthRequest
    {
        [Required]
        public string Token { get; set; }
    }
}