namespace DevBoard.Domain.Entity;

public class User
{
    public Guid UserId { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
   // public string Password { get; set; }    
}