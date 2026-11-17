namespace BloodDonor.Application.DonorRequests
{
    public enum DonorRequestErrorType
    {
        None,
        NotAuthorized,
        BloodRequestNotFound,
        BloodRequestNotOpen,
        DonorRequestNotFound,
        DonorsNotEligible,
        DonorAlreadyRequested,
        TooManyDonorRequests,
        InvalidTransition,
        DonorRequestNotAccepted
    }
}