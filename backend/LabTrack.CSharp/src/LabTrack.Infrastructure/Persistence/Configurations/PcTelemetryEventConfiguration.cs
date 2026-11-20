using LabTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabTrack.Infrastructure.Persistence.Configurations;

public class PcTelemetryEventConfiguration : IEntityTypeConfiguration<PcTelemetryEvent>
{
    public void Configure(EntityTypeBuilder<PcTelemetryEvent> builder)
    {
        builder.HasKey(e => e.Id);
    }
}
