namespace etickets_aspnet_api.Areas.Admin.DTOs.Response
{
    public class CategoryResponse
    {
        public int Id { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(10)]
        public string Name { get; set; } = null!;
    }
}
