namespace BloodDonor.Domain.Constants
{
    public static class Roles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string HospitalAdmin = "HospitalAdmin";
        public const string HospitalStaff = "HospitalStaff";
        public const string Donor = "Donor";
        public const string Patient = "Patient";

        public static readonly IReadOnlyList<string> All = new[]
        {
            SuperAdmin,
            HospitalAdmin,
            HospitalStaff,
            Donor,
            Patient
        };
    }
}