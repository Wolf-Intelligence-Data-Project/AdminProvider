using System;
using System.ComponentModel.DataAnnotations;

namespace AdminProvider.ModeratorsManagement.Utillities;

public class RoleValidationAttribute : ValidationAttribute
{
    private readonly string[] _allowedRoles = { "Admin", "Moderator" };

    public override bool IsValid(object value)
    {
        if (value == null)
            return false;

        var role = value.ToString();
        return Array.Exists(_allowedRoles, r => r.Equals(role, StringComparison.OrdinalIgnoreCase));
    }
}