using LabTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabTrack.Infrastructure.Persistence.Configurations;

public class WorkstationConfiguration : IEntityTypeConfiguration<Workstation>
{
    public void Configure(EntityTypeBuilder<Workstation> builder)
    {
        builder.ToTable("Workstation");
    }
}
