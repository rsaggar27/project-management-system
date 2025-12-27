namespace PMS.Api.Models;

public class FileAttachment
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = null!;
    public string StorageKey { get; set; } = null!;

    public Guid UploadedById { get; set; }
    public User UploadedBy { get; set; } = null!;

    public Guid? ProjectId { get; set; }
    public Project? Project { get; set; }

    public Guid? TaskId { get; set; }
    public TaskItem? Task { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
