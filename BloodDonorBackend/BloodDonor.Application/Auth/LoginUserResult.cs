namespace BloodDonor.Application.Auth
{
    public class LoginUserResult
    {
        public bool Succeeded { get; private set; }
        public LoginErrorType ErrorType { get; private set; } = LoginErrorType.None;
        public string? ErrorMessage { get; private set; }
        public LoginUserResponse? Response { get; private set; }

        public static LoginUserResult Success(LoginUserResponse response)
        {
            return new LoginUserResult
            {
                Succeeded = true,
                Response = response
            };
        }

        public static LoginUserResult Failure(LoginErrorType errorType, string errorMessage)
        {
            return new LoginUserResult
            {
                Succeeded = false,
                ErrorType = errorType,
                ErrorMessage = errorMessage
            };
        }
    }
}