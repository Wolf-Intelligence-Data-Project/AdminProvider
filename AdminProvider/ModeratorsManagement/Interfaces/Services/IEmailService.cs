namespace AdminProvider.ModeratorsManagement.Interfaces.Services;

public interface IEmailService
{
    void SendTemporaryPasswordEmail(string email, string temporaryPassword);
}