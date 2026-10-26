namespace BloodDonor.Application.Hospitals
{
    public interface IUpdateHospitalService
    {
        Task<HospitalResult> UpdateHospitalAsync(
            Guid hospitalId,
            string callerUserId,
            UpdateHospitalRequest request);
    }
}