using PMS.Api.Services;

namespace PMS.Api.Services;

public class EmailDispatcher
{
    private readonly EmailService _email;

    public EmailDispatcher(EmailService email)
    {
        _email = email;
    }

    public void Send(
        string to,
        string subject,
        string body)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await _email.SendAsync(to, subject, body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMAIL FAILED] {ex.Message}");
            }
        });
    }
}
