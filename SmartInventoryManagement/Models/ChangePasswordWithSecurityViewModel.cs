using System.ComponentModel.DataAnnotations;

public class ChangePasswordWithSecurityViewModel
{
    [Required]
    public string SecurityAnswer { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; }
}