using AdminProvider.ModeratorsManagement.Data.Entities;

namespace AdminProvider.ModeratorsManagement.Models.Responses;

public class SignInResponse
{
    public bool Success { get; set; }

    //public bool RedirectToVerification { get; set; }
    public string Message { get; set; }
    public AdminEntity Admin { get; set; }
    public string ErrorMessage { get; set; }
}
