
using DevBoard.API.EndPoints;
using DevBoard.Application.Interfaces;
using DevBoard.Infrastructure.Persistance;
using DevBoard.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

namespace DevBoard.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();
        
        Console.WriteLine(
            builder.Configuration.GetConnectionString("DefaultConnection"));
        
        builder.Services.AddDbContext<AppDbContext>(options => 
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        
        builder.Services.AddScoped<IBoardRepository, BoardRepository>();
       
        

        var app = builder.Build();

        app.MapOpenApi();
        app.MapScalarApiReference(); // available at /scalar/v1

        app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
            .WithName("HealthCheck")
            .WithTags("System");
        app.MapBoardEndpoints();
        app.Run();
    }
}
