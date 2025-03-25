namespace AdminProvider.ModeratorsManagement.Interfaces.Services;

public interface ISignOutService
{
    Task<bool> SignOutAsync(string token);
}