using LabTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabTrack.Infrastructure.Persistence.Configurations;

public class InstructorCommunicationConfiguration : IEntityTypeConfiguration<InstructorCommunication>
{
    public void Configure(EntityTypeBuilder<InstructorCommunication> builder)
    {
        builder.HasKey(e => e.Id);
    }
}
