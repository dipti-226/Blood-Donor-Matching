using BloodDonor.Domain.Entities;
using BloodDonor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BloodDonor.Infrastructure.Matching
{
    internal static class MatchCandidateQuery
    {
        // The single definition of "who can currently be matched". Both the matching list and
        // the sending of donor requests use it, so the two can never disagree.
        // Based on a VERIFIED blood group, opt-in availability, compatibility and city only.
        // This is not a medical eligibility decision.
        public static IQueryable<MatchCandidate> Build(ApplicationDbContext dbContext, List<BloodGroup> compatibleGroups, string cityKey)
        {
            // VerifiedBloodGroup is a nullable column with a custom converter. Checking it against a
            // list of non-nullable values (Contains(x.VerifiedBloodGroup!.Value)) makes EF Core throw
            // "No coercion operator is defined between ... ValueComparer". Comparing nullable with
            // nullable gives EF matching types and avoids it.
            var groups = compatibleGroups.Select(g => (BloodGroup?)g).ToList();

            return
                from v in dbContext.DonorBloodGroupVerifications.AsNoTracking()
                join p in dbContext.DonorProfiles.AsNoTracking() on v.UserId equals p.UserId
                where v.Status == VerificationStatus.Verified
                      && p.IsAvailableForRequests
                      && groups.Contains(v.VerifiedBloodGroup)
                      && p.City.Trim().ToLower() == cityKey
                select new MatchCandidate
                {
                    ProfileId = p.Id,
                    VerifiedBloodGroup = v.VerifiedBloodGroup,
                    VerifiedAt = v.VerifiedAt,
                    City = p.City,
                    Area = p.Area
                };
        }
    }
}