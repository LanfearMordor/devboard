using DevBoard.Domain.Entity;

namespace DevBoard.Application.Interfaces;

public interface IBoardRepository
{
    Task<List<Board>> GetAllAsync(CancellationToken ct = default);
    Task<Board?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Board board, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
    
}