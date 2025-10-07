namespace etickets_aspnet_api.Areas.Admin.DTOs.Request
{
    public class CategoryRequest
    {
        [Required]
        [MinLength(3)]
        [MaxLength(10)]
        public string Name { get; set; } = null!;

    }
}
