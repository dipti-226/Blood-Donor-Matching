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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new DonorProfileConfiguration());
            builder.ApplyConfiguration(new DonorBloodGroupVerificationConfiguration());
            builder.ApplyConfiguration(new HospitalConfiguration());
            builder.ApplyConfiguration(new HospitalUserConfiguration());
        }
    }
}