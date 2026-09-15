using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BloodDonor.Infrastructure.Persistence.Configurations
{
    public class DonorProfileConfiguration : IEntityTypeConfiguration<DonorProfile>
    {
        public void Configure(EntityTypeBuilder<DonorProfile> builder)
        {
            builder.ToTable("DonorProfiles");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.HasIndex(d => d.UserId)
                .IsUnique();

            builder.HasOne<ApplicationUser>()
                .WithOne()
                .HasForeignKey<DonorProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(d => d.FullName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(d => d.Gender)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(d => d.Address)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(d => d.City)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(d => d.Area)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(d => d.Pincode)
                .IsRequired()
                .HasMaxLength(10);
        }
    }
}