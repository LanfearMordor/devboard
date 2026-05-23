using DevBoard.Application.Interfaces;
using DevBoard.Domain.Entity;

namespace DevBoard.API.EndPoints;

public static class BoardEndpoints
{
    public static void MapBoardEndpoints(this WebApplication app)
    {
        var grp = app.MapGroup("/api/v1/boards").WithTags("Boards");
        grp.MapGet("/", async (IBoardRepository repo, CancellationToken ct) =>
            Results.Ok(await repo.GetAllAsync(ct)));

        grp.MapGet("/{id:guid}", async (Guid id, IBoardRepository repo, CancellationToken ct) =>
        {

            var board = await repo.GetByIdAsync(id, ct);
            return board is null ? Results.NotFound() : Results.Ok(board);
        });

        grp.MapPost("/", async (Board board, IBoardRepository repo, CancellationToken ct) =>
        {
            board.Id = Guid.NewGuid();
            board.CreatedAt = DateTime.Now;
            await repo.AddAsync(board, ct);
            await repo.SaveAsync(ct);
            return Results.Created($"/api/v1/boards/{board.Id}", board);
            

        });

    }
}