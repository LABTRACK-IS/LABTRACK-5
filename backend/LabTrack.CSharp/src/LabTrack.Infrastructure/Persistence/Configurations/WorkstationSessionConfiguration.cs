using LabTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabTrack.Infrastructure.Persistence.Configurations;

public class WorkstationSessionConfiguration : IEntityTypeConfiguration<WorkstationSession>
{
    public void Configure(EntityTypeBuilder<WorkstationSession> builder)
    {
        builder.HasKey(e => e.Id);
    }
}
