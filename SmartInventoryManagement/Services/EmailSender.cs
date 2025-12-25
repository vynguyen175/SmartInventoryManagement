using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

/// <summary>
/// A no-op email sender that logs emails to console instead of sending them.
/// Replace with a real email service (SendGrid, SMTP, etc.) for production.
/// </summary>
public class EmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // Log the email to console instead of sending it
        Console.WriteLine("========== EMAIL ==========");
        Console.WriteLine($"To: {email}");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"Body: {htmlMessage}");
        Console.WriteLine("===========================");
        
        return Task.CompletedTask;
    }
}