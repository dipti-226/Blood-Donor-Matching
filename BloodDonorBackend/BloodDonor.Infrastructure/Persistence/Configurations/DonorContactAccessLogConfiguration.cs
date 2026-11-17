using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BloodDonor.Infrastructure.Persistence.Configurations
{
    public class DonorContactAccessLogConfiguration : IEntityTypeConfiguration<DonorContactAccessLog>
    {
        public void Configure(EntityTypeBuilder<DonorContactAccessLog> builder)
        {
            builder.ToTable("DonorContactAccessLogs");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.DonorRequestId)
                .IsRequired();

            builder.Property(l => l.AccessedByUserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(l => l.AccessedAt)
                .IsRequired();

            builder.HasIndex(l => l.DonorRequestId);

            builder.HasOne<DonorRequest>()
                .WithMany()
                .HasForeignKey(l => l.DonorRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(l => l.AccessedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}