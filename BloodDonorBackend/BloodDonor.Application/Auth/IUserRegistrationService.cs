namespace BloodDonor.Application.Auth
{
    public interface IUserRegistrationService
    {
        Task<RegisterUserResult> RegisterAsync(RegisterUserRequest request);
    }
}