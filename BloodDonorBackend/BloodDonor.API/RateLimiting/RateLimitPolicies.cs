namespace BloodDonor.API.RateLimiting
{
    public static class RateLimitPolicies
    {
        // Login and register: per client IP.
        public const string Auth = "auth";

        // Donor contact disclosure: per user.
        public const string Contact = "contact";
    }
}