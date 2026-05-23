using Microsoft.EntityFrameworkCore;
using DevBoard.Domain.Entity;

namespace DevBoard.Infrastructure.Persistance;

//injecting DI ,shifts the responsibility of configuring the database from the
//DbContext class itself to the application's startup configuration


    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Board> Boards => Set<Board>();
        public DbSet<TaskItem> TaskItems => Set<TaskItem>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
