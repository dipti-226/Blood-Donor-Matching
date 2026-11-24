namespace BloodDonor.Application.Hospitals
{
    public interface IHospitalAdminQueryService
    {
        Task<HospitalAdminListResult> GetAdminsAsync(Guid hospitalId);
    }
}