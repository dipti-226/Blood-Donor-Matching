namespace BloodDonor.Application.Auth
{
    public interface ILoginService
    {
        Task<LoginUserResult> LoginAsync(LoginUserRequest request);
    }
}