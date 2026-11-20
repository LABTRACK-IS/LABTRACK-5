using LabTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabTrack.Infrastructure.Persistence.Configurations;

public class NetworkDeviceConfiguration : IEntityTypeConfiguration<NetworkDevice>
{
    public void Configure(EntityTypeBuilder<NetworkDevice> builder)
    {
        builder.HasKey(e => e.Id);
    }
}
