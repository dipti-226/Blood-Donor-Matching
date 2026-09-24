namespace BloodDonor.Application.Hospitals
{
    public interface IUpdateHospitalStatusService
    {
        Task<HospitalResult> UpdateStatusAsync(Guid id, UpdateHospitalStatusRequest request);
    }
}