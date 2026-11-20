using LabTrack.Domain.Entities;
using LabTrack.Domain.Entities;
using LabTrack.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace LabTrack.Infrastructure.Persistence.Repositories;

public class ReplacementRepository : IReplacementRepository
{
    private readonly LabTrackDbContext _context;

    public ReplacementRepository(LabTrackDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    private DbSet<ComponentReplacement> Replacements => _context.Set<ComponentReplacement>();

    public async Task<ComponentReplacement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Replacements
            .Include("Asset")
            .Include("RequestedByUser")
            .Include("Technician")
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ComponentReplacement>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Replacements
            .Include("Asset")
            .Include("RequestedByUser")
            .Include("Technician")
            
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<ComponentReplacement> AddAsync(ComponentReplacement replacement, CancellationToken cancellationToken = default)
    {
        await Replacements.AddAsync(replacement, cancellationToken);
        return replacement;
    }

    public Task UpdateAsync(ComponentReplacement replacement, CancellationToken cancellationToken = default)
    {
        Replacements.Update(replacement);
        return RepositoryExecutionEngine.CompleteAsync();
    }

    public Task DeleteAsync(ComponentReplacement replacement, CancellationToken cancellationToken = default)
    {
        Replacements.Remove(replacement);
        return RepositoryExecutionEngine.CompleteAsync();
    }

    public async Task<IReadOnlyList<ComponentReplacement>> GetByAssetIdAsync(Guid assetId, CancellationToken cancellationToken = default)
    {
        return await Replacements
            .Include("RequestedByUser")
            .Include("Technician")
            .Where(r => EF.Property<Guid>(r, "AssetId") == assetId)
            
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ComponentReplacement>> GetByWorkstationIdAsync(Guid workstationId, CancellationToken cancellationToken = default)
    {
        return await Replacements
            .Include("Asset")
            .Where(r => EF.Property<Guid?>(r, "WorkstationId") == workstationId)
            
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ComponentReplacement>> GetByRepairTicketIdAsync(Guid repairTicketId, CancellationToken cancellationToken = default)
    {
        return await Replacements
            .Include("Asset")
            .Include("RequestedByUser")
            .Where(r => EF.Property<Guid>(r, "RepairTicketId") == repairTicketId)
            
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ComponentReplacement>> GetByLaboratoryIdAsync(Guid laboratoryId, CancellationToken cancellationToken = default)
    {
        return await Replacements
            .Include("Asset")
            .Where(r => EF.Property<Guid?>(r, "LaboratoryId") == laboratoryId)
            
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ComponentReplacement>> GetByTechnicianIdAsync(Guid technicianId, CancellationToken cancellationToken = default)
    {
        return await Replacements
            .Include("Asset")
            .Where(r => EF.Property<Guid?>(r, "TechnicianId") == technicianId)
            
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ComponentReplacement>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 20;

        return await Replacements
            .Include("Asset")
            .Include("RequestedByUser")
            .Where(r => EF.Property<DateTime>(r, "CreatedAt") >= startDate
                     && EF.Property<DateTime>(r, "CreatedAt") <= endDate)
            
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalReplacementCostByAssetAsync(Guid assetId, CancellationToken cancellationToken = default)
    {
        var total = await Replacements
            .Where(r => EF.Property<Guid>(r, "AssetId") == assetId)
            .SumAsync(r => EF.Property<decimal?>(r, "Cost"), cancellationToken);

        return total ?? 0m;
    }
}


