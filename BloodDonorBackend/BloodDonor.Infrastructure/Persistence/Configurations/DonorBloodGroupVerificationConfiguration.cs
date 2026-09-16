using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BloodDonor.Infrastructure.Persistence.Configurations
{
    public class DonorBloodGroupVerificationConfiguration : IEntityTypeConfiguration<DonorBloodGroupVerification>
    {
        public void Configure(EntityTypeBuilder<DonorBloodGroupVerification> builder)
        {
            builder.ToTable("DonorBloodGroupVerifications");

            builder.HasKey(v => v.Id);

            builder.Property(v => v.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.HasIndex(v => v.UserId)
                .IsUnique();

            builder.HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<DonorBloodGroupVerification>(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            var bloodGroupConverter = new Dictionary<BloodGroup, string>
            {
                { BloodGroup.APlus, "A+" },
                { BloodGroup.AMinus, "A-" },
                { BloodGroup.BPlus, "B+" },
                { BloodGroup.BMinus, "B-" },
                { BloodGroup.ABPlus, "AB+" },
                { BloodGroup.ABMinus, "AB-" },
                { BloodGroup.OPlus, "O+" },
                { BloodGroup.OMinus, "O-" }
            };
            var bloodGroupReverseConverter = bloodGroupConverter.ToDictionary(kv => kv.Value, kv => kv.Key);

            builder.Property(v => v.DeclaredBloodGroup)
                .HasConversion(
                    bg => bloodGroupConverter[bg],
                    s => bloodGroupReverseConverter[s])
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(v => v.VerifiedBloodGroup)
                .HasConversion(
                    bg => bg.HasValue ? bloodGroupConverter[bg.Value] : null,
                    s => s != null ? bloodGroupReverseConverter[s] : (BloodGroup?)null)
                .HasMaxLength(3);

            builder.Property(v => v.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(v => v.VerifiedByUserId)
                .HasMaxLength(450);

            builder.Property(v => v.RejectionReason)
                .HasMaxLength(1000);
        }
    }
}