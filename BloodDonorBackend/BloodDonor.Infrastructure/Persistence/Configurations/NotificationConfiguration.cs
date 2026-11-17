using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BloodDonor.Infrastructure.Persistence.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");

            builder.HasKey(n => n.Id);

            builder.Property(n => n.RecipientUserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(n => n.Type)
                .HasConversion<string>()
                .HasMaxLength(40)
                .IsRequired();

            builder.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(n => n.IsRead)
                .IsRequired();

            builder.Property(n => n.CreatedAt)
                .IsRequired();

            builder.HasIndex(n => new { n.RecipientUserId, n.IsRead, n.CreatedAt });

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(n => n.RecipientUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}