using DevBoard.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DevBoard.Infrastructure.Persistance.Configurations;

public class BoardConfiguration : IEntityTypeConfiguration<Board>
{
    public void Configure(EntityTypeBuilder<Board> builder){
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Name).HasMaxLength(100);
        builder.Property( b=> b.Description).IsRequired().HasMaxLength(500);
        builder.HasMany(b => b.Tasks)
            .WithOne()
            .HasForeignKey(t => t.BoardId)
            .OnDelete(DeleteBehavior.Cascade);
       }
}