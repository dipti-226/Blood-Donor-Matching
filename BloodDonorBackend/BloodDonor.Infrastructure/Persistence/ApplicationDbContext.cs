using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.Identity;
using BloodDonor.Infrastructure.Persistence.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BloodDonor.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<DonorProfile> DonorProfiles => Set<DonorProfile>();
        public DbSet<DonorBloodGroupVerification> DonorBloodGroupVerifications => Set<DonorBloodGroupVerification>();
        public DbSet<Hospital> Hospitals => Set<Hospital>();
        public DbSet<HospitalUser> HospitalUsers => Set<HospitalUser>();
        public DbSet<BloodRequest> BloodRequests => Set<BloodRequest>();
        public DbSet<DonorRequest> DonorRequests => Set<DonorRequest>();
        public DbSet<DonorContactAccessLog> DonorContactAccessLogs => Set<DonorContactAccessLog>();
        public DbSet<Notification> Notifications => Set<Notification>();
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new DonorProfileConfiguration());
            builder.ApplyConfiguration(new DonorBloodGroupVerificationConfiguration());
            builder.ApplyConfiguration(new HospitalConfiguration());
            builder.ApplyConfiguration(new HospitalUserConfiguration());
            builder.ApplyConfiguration(new BloodRequestConfiguration());
            builder.ApplyConfiguration(new DonorRequestConfiguration());
            builder.ApplyConfiguration(new DonorContactAccessLogConfiguration());
            builder.ApplyConfiguration(new NotificationConfiguration());
        }
    }
}