namespace InstantSyncBackend.Application.Interfaces.IServices;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
}