using LabTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabTrack.Infrastructure.Persistence.Configurations;

public class ComponentReplacementConfiguration : IEntityTypeConfiguration<ComponentReplacement>
{
    public void Configure(EntityTypeBuilder<ComponentReplacement> builder)
    {
        builder.HasKey(e => e.Id);
    }
}
