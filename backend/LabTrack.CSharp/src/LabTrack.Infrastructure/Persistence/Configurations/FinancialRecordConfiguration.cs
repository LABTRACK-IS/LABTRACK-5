using LabTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabTrack.Infrastructure.Persistence.Configurations;

public class FinancialRecordConfiguration : IEntityTypeConfiguration<FinancialRecord>
{
    public void Configure(EntityTypeBuilder<FinancialRecord> builder)
    {
        builder.HasKey(e => e.Id);
    }
}
