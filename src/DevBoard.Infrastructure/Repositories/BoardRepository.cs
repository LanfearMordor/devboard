using DevBoard.Application.Interfaces;
using DevBoard.Domain.Entity;
using DevBoard.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace DevBoard.Infrastructure.Repositories;

public class BoardRepository(AppDbContext context) : IBoardRepository
{
    private readonly AppDbContext _context = context;

    public async Task<List<Board>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Boards.AsNoTracking().ToListAsync(ct);
    }

    public async Task<Board?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Boards.FindAsync([id], ct);
        
    }

    public async Task AddAsync(Board board, CancellationToken ct = default)
    {
        await _context.Boards.AddAsync(board, ct);
    }

    public async Task SaveAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}