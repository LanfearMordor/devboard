using DevBoard.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevBoard.Infrastructure.Persistance.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Description).HasMaxLength(100);
        builder.Property(t => t.Title).HasMaxLength(100).IsRequired();
        builder.Property(t => t.CreatedBy);
        builder.Property(t => t.CreatedAt).IsRequired().HasColumnType("datetime");
        //builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.TaskType).IsRequired().HasConversion<string>();
        builder.Property(t => t.Status).HasConversion<string>();
        builder.Property(t => t.Priority).IsRequired().HasConversion<string>();
        builder.Property(t => t.DueDate).HasColumnType("datetime");
        
    }
    
}
