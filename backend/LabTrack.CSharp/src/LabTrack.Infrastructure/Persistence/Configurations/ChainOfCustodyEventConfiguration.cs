using LabTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabTrack.Infrastructure.Persistence.Configurations;

public class ChainOfCustodyEventConfiguration : IEntityTypeConfiguration<ChainOfCustodyEvent>
{
    public void Configure(EntityTypeBuilder<ChainOfCustodyEvent> builder)
    {
        builder.HasKey(e => e.Id);
    }
}
