namespace BloodDonor.Domain.Constants
{
    public static class PubliclyRegisterableRoles
    {
        public static readonly IReadOnlyList<string> All = new[]
        {
            Roles.Donor,
            Roles.Patient
        };

        public static bool Contains(string roleName)
        {
            return All.Contains(roleName);
        }
    }
}