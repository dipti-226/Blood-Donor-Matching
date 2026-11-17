using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BloodDonor.Infrastructure.Persistence.Configurations
{
    public class BloodRequestConfiguration : IEntityTypeConfiguration<BloodRequest>
    {
        public void Configure(EntityTypeBuilder<BloodRequest> builder)
        {
            builder.ToTable("BloodRequests");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.HospitalId)
                .IsRequired();

            builder.Property(r => r.CreatedByUserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(r => r.ClosedByUserId)
                .HasMaxLength(450);

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

            builder.Property(r => r.PatientBloodGroup)
                .HasConversion(
                    bg => bloodGroupConverter[bg],
                    s => bloodGroupReverseConverter[s])
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(r => r.Component)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(r => r.UnitsRequired)
                .IsRequired();

            builder.Property(r => r.Urgency)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(r => r.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(r => r.CancellationReason)
                .HasMaxLength(1000);

            builder.HasIndex(r => new { r.HospitalId, r.Status, r.CreatedAt });

            builder.HasOne<Hospital>()
                .WithMany()
                .HasForeignKey(r => r.HospitalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(r => r.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(r => r.ClosedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}