namespace BloodDonor.Application.Hospitals
{
    public class HospitalStaffResponse
    {
        public Guid HospitalUserId { get; set; }
        public Guid HospitalId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}