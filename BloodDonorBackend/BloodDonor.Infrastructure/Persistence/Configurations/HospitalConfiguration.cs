using BloodDonor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BloodDonor.Infrastructure.Persistence.Configurations
{
    public class HospitalConfiguration : IEntityTypeConfiguration<Hospital>
    {
        public void Configure(EntityTypeBuilder<Hospital> builder)
        {
            builder.ToTable("Hospitals");

            builder.HasKey(h => h.Id);

            builder.Property(h => h.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(h => h.RegistrationNumber)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(h => h.RegistrationNumber)
                .IsUnique();

            builder.Property(h => h.Address)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(h => h.City)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(h => h.Area)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(h => h.Pincode)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(h => h.ContactNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(h => h.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(h => h.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
        }
    }
}