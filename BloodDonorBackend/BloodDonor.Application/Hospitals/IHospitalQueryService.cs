namespace BloodDonor.Application.Hospitals
{
    public interface IHospitalQueryService
    {
        Task<HospitalResult> GetByIdAsync(Guid id, string callerUserId, bool isSuperAdmin);
        Task<IReadOnlyList<HospitalResponse>> GetAllAsync();
    }
}