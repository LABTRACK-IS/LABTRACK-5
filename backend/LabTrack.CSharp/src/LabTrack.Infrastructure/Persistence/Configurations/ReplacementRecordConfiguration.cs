using LabTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabTrack.Infrastructure.Persistence.Configurations;

public class ReplacementRecordConfiguration : IEntityTypeConfiguration<ReplacementRecord>
{
    public void Configure(EntityTypeBuilder<ReplacementRecord> builder)
    {
        builder.HasKey(e => e.Id);
    }
}
