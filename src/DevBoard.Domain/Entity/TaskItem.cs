using DevBoard.Domain.Enum;
using TaskStatus = System.Threading.Tasks.TaskStatus;

namespace DevBoard.Domain.Entity;

public class TaskItem
{
    public required Guid Id { get; set; }
    public Guid BoardId { get; set; }
    public string? Description { get; set; }
    public required string Title { get; set; }
    public required Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public TaskType TaskType { get; set; } //enum
    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    
    
}