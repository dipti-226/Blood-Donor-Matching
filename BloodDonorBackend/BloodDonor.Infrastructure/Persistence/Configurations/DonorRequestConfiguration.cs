using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BloodDonor.Infrastructure.Persistence.Configurations
{
    public class DonorRequestConfiguration : IEntityTypeConfiguration<DonorRequest>
    {
        public void Configure(EntityTypeBuilder<DonorRequest> builder)
        {
            builder.ToTable("DonorRequests");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.BloodRequestId)
                .IsRequired();

            builder.Property(r => r.DonorProfileId)
                .IsRequired();

            builder.Property(r => r.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(r => r.CreatedByUserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(r => r.ContactNumber)
                .HasMaxLength(20);

            // A donor can never be asked twice for the same blood request.
            builder.HasIndex(r => new { r.BloodRequestId, r.DonorProfileId })
                .IsUnique();

            builder.HasIndex(r => new { r.BloodRequestId, r.Status });

            builder.HasIndex(r => new { r.DonorProfileId, r.Status });

            builder.HasOne<BloodRequest>()
                .WithMany()
                .HasForeignKey(r => r.BloodRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<DonorProfile>()
                .WithMany()
                .HasForeignKey(r => r.DonorProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(r => r.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}