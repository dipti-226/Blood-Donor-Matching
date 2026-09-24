namespace BloodDonor.Application.Hospitals
{
    public interface IHospitalQueryService
    {
        Task<HospitalResult> GetByIdAsync(Guid id);
        Task<IReadOnlyList<HospitalResponse>> GetAllAsync();
    }
}