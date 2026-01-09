using PMS.Api.Data;
using PMS.Api.Models;

namespace PMS.Api.Services;

public class NotificationService
{
    private readonly AppDbContext _db;
    private readonly EmailDispatcher _emailDispatcher;

    public NotificationService(AppDbContext db, EmailDispatcher emailDispatcher)
    {
        _db = db;
        _emailDispatcher = emailDispatcher;
    }

    public async Task CreateAsync(
        Guid userId,
        string type,
        string message)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Message = message
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();

        if (type == "TaskAssigned")
        {
            var user = await _db.Users.FindAsync(userId);
            if (user != null)
            {
                _emailDispatcher.Send(
                    to: user.Email,
                    subject: "Task Assigned",
                    body: message
                );
            }
        }
    }
}
