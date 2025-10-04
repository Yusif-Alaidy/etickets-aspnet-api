namespace etickets_aspnet_api.Areas.Identity.DTOs.Request
{
    public class NewPasswordRequest
    {
        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [Required, DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;
        public string ApplicationUserId { get; set; } = string.Empty;
        public string OTPNumber { get; set; } = string.Empty;

    }
}
