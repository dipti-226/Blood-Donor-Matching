namespace BloodDonor.Application.Auth
{
    public interface ITokenService
    {
        string GenerateAccessToken(string userId, string email, IReadOnlyList<string> roles);
    }
}