namespace BloodDonor.Application.Auth
{
    public enum RegistrationErrorType
    {
        None,
        RoleNotAllowed,
        EmailAlreadyExists,
        IdentityCreationFailed
    }
}