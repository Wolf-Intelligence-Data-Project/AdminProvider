﻿using System.ComponentModel.DataAnnotations;

namespace AdminProvider.ModeratorsManagement.Models.Requests;

public class FirstPasswordChangeRequest
{
    [Required]
    public string AdminId { get; set; }

    [Required(ErrorMessage = "Lösenord krävs.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Lösenordet måste vara minst 8 tecken långt.")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[!@#$%^&*()_+={}\[\]:;'<>?\/.,]).{8,}$",
       ErrorMessage = "Lösenordet måste innehålla minst en bokstav, en siffra och ett specialtecken.")]
    public string Password { get; set; }

    public string ConfirmPassword { get; set; }
}
