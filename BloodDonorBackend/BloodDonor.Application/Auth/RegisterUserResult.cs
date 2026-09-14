namespace BloodDonor.Application.Auth
{
    public class RegisterUserResult
    {
        public bool Succeeded { get; private set; }
        public RegistrationErrorType ErrorType { get; private set; } = RegistrationErrorType.None;
        public IReadOnlyList<string> Errors { get; private set; } = Array.Empty<string>();
        public RegisterUserResponse? Response { get; private set; }

        public static RegisterUserResult Success(RegisterUserResponse response)
        {
            return new RegisterUserResult
            {
                Succeeded = true,
                Response = response
            };
        }

        public static RegisterUserResult Failure(RegistrationErrorType errorType, IEnumerable<string> errors)
        {
            return new RegisterUserResult
            {
                Succeeded = false,
                ErrorType = errorType,
                Errors = errors.ToList()
            };
        }
    }
}