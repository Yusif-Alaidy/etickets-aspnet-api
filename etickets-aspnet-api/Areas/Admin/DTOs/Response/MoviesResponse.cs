namespace etickets_aspnet_api.Areas.Admin.DTOs.Response
{
    public class MoviesResponse
    {
        public int Id { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(10)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        public string ImgUrl { get; set; } = "default.jpg";
        public int? Quantity { get; set; }

        public string? TrailerUrl { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int MovieStatus { get; set; }

        public string? CinemaName { get; set; }

        public string? CategoryName { get; set; }
        public List<int>? ActorsIds { get; set; } = new List<int>();

    }
}
