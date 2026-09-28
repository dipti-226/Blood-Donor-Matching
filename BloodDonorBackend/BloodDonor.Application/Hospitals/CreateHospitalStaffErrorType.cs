namespace BloodDonor.Application.Hospitals
{
    public enum CreateHospitalStaffErrorType
    {
        None,
        HospitalNotFound,
        NotAuthorizedForHospital,
        EmailAlreadyExists,
        IdentityCreationFailed
    }
}