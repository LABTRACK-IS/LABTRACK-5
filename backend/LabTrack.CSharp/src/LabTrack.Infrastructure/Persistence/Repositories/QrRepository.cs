using LabTrack.Application.Interfaces.Repositories;
using LabTrack.Domain.Entities;
using LabTrack.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace LabTrack.Infrastructure.Persistence.Repositories;

public class QrRepository : IQrRepository
{
    private readonly LabTrackDbContext _context;

    public QrRepository(LabTrackDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // ─── IGenericRepository<QrCode> ───

    public async Task<QrCode?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.QrCodes
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<QrCode>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.QrCodes
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<QrCode> AddAsync(QrCode entity, CancellationToken cancellationToken = default)
    {
        await _context.QrCodes.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(QrCode entity, CancellationToken cancellationToken = default)
    {
        _context.QrCodes.Update(entity);
        return RepositoryExecutionEngine.CompleteAsync();
    }

    public Task DeleteAsync(QrCode entity, CancellationToken cancellationToken = default)
    {
        _context.QrCodes.Remove(entity);
        return RepositoryExecutionEngine.CompleteAsync();
    }

    // ─── IQrRepository specific ───

    public Task<QrCode?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        // QrCode has no Token property — return null
        return RepositoryExecutionEngine.ResolveAsync<QrCode?>(null);
    }

    public Task<QrCode?> GetActiveByWorkstationIdAsync(
        Guid workstationId,
        CancellationToken cancellationToken = default)
    {
        // QrCode has no WorkstationId property — return null
        return RepositoryExecutionEngine.ResolveAsync<QrCode?>(null);
    }

    public Task<IReadOnlyList<QrCode>> GetActiveByLaboratoryIdAsync(
        Guid laboratoryId,
        CancellationToken cancellationToken = default)
    {
        // QrCode has no LaboratoryId property — return empty list
        return RepositoryExecutionEngine.ResolveAsync<IReadOnlyList<QrCode>>(RepositoryExecutionEngine.Empty<QrCode>());
    }

    public Task<bool> IsTokenValidAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        // QrCode has no Token property — return false
        return RepositoryExecutionEngine.ResolveAsync(false);
    }

    public Task DeactivateExistingTokensForWorkstationAsync(
        Guid workstationId,
        CancellationToken cancellationToken = default)
    {
        // QrCode has no WorkstationId property — no-op
        return RepositoryExecutionEngine.CompleteAsync();
    }

    public Task RevokeExpiredTokensAsync(
        DateTime cutoffUtc,
        CancellationToken cancellationToken = default)
    {
        // QrCode has no ExpiresAtUtc property — no-op
        return RepositoryExecutionEngine.CompleteAsync();
    }
}
