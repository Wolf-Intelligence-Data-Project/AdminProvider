namespace AdminProvider.ModeratorsManagement.Models.Responses;

public class AuthStatus
{
    public bool IsAuthenticated { get; set; }
    public string Role { get; set; }
    public string ErrorMessage { get; set; }
}