using LabTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabTrack.Infrastructure.Persistence.Configurations;

public class QrSessionConfiguration : IEntityTypeConfiguration<QrSession>
{
    public void Configure(EntityTypeBuilder<QrSession> builder)
    {
        builder.ToTable("QrSession");
    }
}
