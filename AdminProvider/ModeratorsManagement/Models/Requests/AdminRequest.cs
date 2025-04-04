using System.ComponentModel.DataAnnotations;

namespace AdminProvider.ModeratorsManagement.Models.Requests;

public class AdminRequest
{
    [Required(ErrorMessage = "E-postadress är obligatorisk.")]
    [EmailAddress(ErrorMessage = "E-postadressen är ogiltig.")]
    [StringLength(256, ErrorMessage = "E-postadressen får inte vara längre än 256 tecken.")]
    [RegularExpression(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$", ErrorMessage = "E-postadressen följer inte standardformatet.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "E-postadress är obligatorisk.")]
    [EmailAddress(ErrorMessage = "E-postadressen är ogiltig.")]
    [Compare("Password", ErrorMessage = "Lösenorden matchar inte.")]
    public string ConfirmEmail { get; set; }

    [Required(ErrorMessage = "Lösenord krävs.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Lösenordet måste vara minst 8 tecken långt.")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[!@#$%^&*()_+={}\[\]:;'<>?\/.,]).{8,}$",
       ErrorMessage = "Lösenordet måste innehålla minst en bokstav, en siffra och ett specialtecken.")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Bekräfta lösenord krävs.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Lösenorden matchar inte.")]
    public string ConfirmPassword { get; set; }

    [Required(ErrorMessage = "Telefonnummer är obligatoriskt.")]
    [RegularExpression(@"^\+46\s7[02369](\s\d{2,3}){3}$", ErrorMessage = "Telefonnummer måste ha formatet +46 7X XXX XX XX eller liknande.")]
    public string PhoneNumber { get; set; }

    [Required]
    public string Role { get; set; }  // "Admin" or "Moderator"

    [Required(ErrorMessage = "Organisationsnummer är obligatoriskt.")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "Organisationsnummer måste vara exakt 10 siffror.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Organisationsnummer får endast innehålla siffror.")]
    public string IdentificationNumber { get; set; }
    [Required]
    public string FullName { get; set; }
}
