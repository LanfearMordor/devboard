namespace DevBoard.Domain.Entity;

public class Notification
{
    //Id, UserId, Message, IsRead, CreatedAt
   public Guid Id { get; set; }
   public int UserId { get; set; }
   public required string Message { get; set; } 
   public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}