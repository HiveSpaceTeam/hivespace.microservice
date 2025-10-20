namespace HiveSpace.UserService.Infrastructure.Settings;

public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 30;

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(SmtpServer) &&
               SmtpPort > 0 &&
               !string.IsNullOrWhiteSpace(SmtpUser) &&
               !string.IsNullOrWhiteSpace(SmtpPassword) &&
               !string.IsNullOrWhiteSpace(FromEmail) &&
               !string.IsNullOrWhiteSpace(FromName);
    }
}