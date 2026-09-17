using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BloodDonor.Infrastructure.Persistence.Configurations
{
    public class HospitalUserConfiguration : IEntityTypeConfiguration<HospitalUser>
    {
        public void Configure(EntityTypeBuilder<HospitalUser> builder)
        {
            builder.ToTable("HospitalUsers");

            builder.HasKey(hu => hu.Id);

            builder.Property(hu => hu.HospitalId)
                .IsRequired();

            builder.Property(hu => hu.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.HasIndex(hu => hu.UserId)
                .IsUnique();

            builder.HasIndex(hu => new { hu.HospitalId, hu.UserId })
                .IsUnique();

            builder.HasOne<Hospital>()
                .WithMany()
                .HasForeignKey(hu => hu.HospitalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<HospitalUser>(hu => hu.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}