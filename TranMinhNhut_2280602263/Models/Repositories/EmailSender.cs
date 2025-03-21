using Microsoft.AspNetCore.Identity.UI.Services;

public class EmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // For now, just return a completed task since we're not implementing actual email sending
        return Task.CompletedTask;
    }
}
