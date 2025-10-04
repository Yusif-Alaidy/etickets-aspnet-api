namespace etickets_aspnet_api.Areas.Identity.DTOs.Request
{
    public class ForgetPasswordRequest
    {
        [Required]
        public string EmailORUserName { get; set; } = string.Empty;
    }
}
