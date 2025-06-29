﻿using AdminProvider.ModeratorsManagement.Utillities;
using System.ComponentModel.DataAnnotations;

namespace AdminProvider.ModeratorsManagement.Data.Entities;

public class AdminEntity
{
    [Key]
    public Guid AdminId { get; set; }

    [Required(ErrorMessage = "E-postadress är obligatorisk.")]
    [EmailAddress(ErrorMessage = "E-postadressen är ogiltig.")]
    [StringLength(256, ErrorMessage = "E-postadressen får inte vara längre än 256 tecken.")]
    [RegularExpression(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$", ErrorMessage = "E-postadressen följer inte standardformatet.")]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    [Required]
    [RoleValidation]
    public string Role { get; set; }  // "Admin" or "Moderator"

    [Required(ErrorMessage = "Organisationsnummer är obligatoriskt.")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "Organisationsnummer måste vara exakt 10 siffror.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Organisationsnummer får endast innehålla siffror.")]
    public string IdentificationNumber { get; set; }

    [Required(ErrorMessage = "Telefonnummer är obligatoriskt.")]
    [RegularExpression(@"^\+46\s7[02369](\s\d{2,3}){3}$", ErrorMessage = "Telefonnummer måste ha formatet +46 7X XXX XX XX eller liknande.")]
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "Fullständigt namn är obligatoriskt.")]
    [StringLength(100, ErrorMessage = "Ansvarig persons namn får inte vara längre än 100 tecken.")]
    [RegularExpression(@"^([A-ZÅÄÖ][a-zåäö]*(?:-[A-ZÅÄÖ][a-zåäö]*)?\s)+([A-ZÅÄÖ][a-zåäö]*(?:-[A-ZÅÄÖ][a-zåäö]*)?)$", ErrorMessage = "Varje ord i ansvarig persons namn måste börja med en stor bokstav. Bindestreck är tillåtet.")]
    [MinLength(2, ErrorMessage = "Ansvarig persons namn måste bestå av minst två ord.")]
    public string FullName { get; set; }

    public bool PasswordChosen { get; set; } = false;
}