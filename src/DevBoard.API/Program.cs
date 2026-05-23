
using DevBoard.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

namespace DevBoard.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();
        builder.Services.AddDbContext<AppDbContext>(options => 
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        

        var app = builder.Build();

        app.MapOpenApi();
        app.MapScalarApiReference(); // available at /scalar/v1

        app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
            .WithName("HealthCheck")
            .WithTags("System");

        app.Run();
    }
}
