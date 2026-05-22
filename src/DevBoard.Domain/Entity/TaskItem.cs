namespace DevBoard.Domain.Entity;

public class TaskItem
{
    public required int Id { get; set; }
    public string Description { get; set; }
    public required string Title { get; set; }
    public required string CreatedBy { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime DueDate { get; set; }
    public required string TaskType { get; set; }
    
}