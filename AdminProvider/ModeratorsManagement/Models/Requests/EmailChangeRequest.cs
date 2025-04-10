using System.ComponentModel.DataAnnotations;

namespace AdminProvider.ModeratorsManagement.Models.Requests;

public class EmailChangeRequest
{

    [Required(ErrorMessage = "E-postadress är obligatorisk.")]
    [EmailAddress(ErrorMessage = "E-postadressen är ogiltig.")]
    [StringLength(256, ErrorMessage = "E-postadressen får inte vara längre än 256 tecken.")]
    [RegularExpression(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$", ErrorMessage = "E-postadressen följer inte standardformatet.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Lösenord krävs.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Lösenordet måste vara minst 8 tecken långt.")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[!@#$%^&*()_+={}\[\]:;'<>?\/.,]).{8,}$",
       ErrorMessage = "Lösenordet måste innehålla minst en bokstav, en siffra och ett specialtecken.")]
    public string Password { get; set; }
}
