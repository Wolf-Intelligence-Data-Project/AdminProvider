using System.Text.Json;
using System.Text;
using AdminProvider.ModeratorsManagement.Interfaces.Services;

namespace AdminProvider.ModeratorsManagement.Services;

public class EmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(HttpClient httpClient, IConfiguration configuration, ILogger<EmailService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async void SendTemporaryPasswordEmail(string email, string temporaryPassword)
    {
        var brevoApiKey = _configuration["Brevo:ApiKey"];
        var senderEmail = _configuration["Brevo:AdminEmail"];

        var emailContent = new
        {
            sender = new { email = senderEmail },
            to = new[] { new { email } },
            subject = "Ditt tillfälliga lösenord",
            htmlContent = $"<p>Ditt tillfälliga lösenord är: <strong>{temporaryPassword}</strong></p><p>Vänligen byt det vid första inloggningen.</p>"
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email")
        {
            Headers = { { "api-key", brevoApiKey } },
            Content = new StringContent(JsonSerializer.Serialize(emailContent), Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Failed to send email to {email}: {response.StatusCode}");
        }
    }
}