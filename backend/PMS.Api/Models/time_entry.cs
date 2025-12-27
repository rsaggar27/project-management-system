namespace PMS.Api.Models;

public class TimeEntry
{
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }
    public TaskItem Task { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public int DurationSeconds { get; set; }
}
