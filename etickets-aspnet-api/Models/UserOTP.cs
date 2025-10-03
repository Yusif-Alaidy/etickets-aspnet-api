namespace etickets_aspnet_api.Models
{
    public class UserOTP
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public string OTPNumber { get; set; }
        public bool IsUsed { get; set; }
        public DateTime ValidTo { get; set; }
    }
}
