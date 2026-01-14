using System.ComponentModel.DataAnnotations;

public class EditUserInfoDto
{
    [Required(ErrorMessage = "First name is required.")]
    [MinLength(2, ErrorMessage = "First name must be at least 2 characters long.")]
    [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
    public required string FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required.")]
    [MinLength(2, ErrorMessage = "Last name must be at least 2 characters long.")]
    [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
    public required string LastName { get; set; }

    [Required(ErrorMessage = "User name is required.")]
    [MinLength(2, ErrorMessage = "User name must be at least 2 characters long.")]
    [MaxLength(50, ErrorMessage = "User name cannot exceed 50 characters.")]
    public required string UserName { get; set; }
    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
    public required string Password { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public required string Email { get; set; }
    
    [Required(ErrorMessage = "Phone number is required.")]
    [Phone(ErrorMessage = "Invalid phone number format.")]
    public required string PhoneNumber { get; set; }
    public required string UserRoles { get; set; } = "Customer";
}