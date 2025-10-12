namespace etickets_aspnet_api.Areas.Admin.DTOs.Request
{
    public class MovieRequest
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
        [Required]
        public int CinemaId { get; set; }
        [Required]
        public int CategoryId { get; set; }
        //public List<int> ActorIds { get; set; } = new List<int>();
        [Required]
        public List<int>? ActorsIds { get; set; } = new List<int>();

        // ✅ Custom validation for start date < end date
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartDate >= EndDate)
            {
                yield return new ValidationResult(
                    "Start Date must be earlier than End Date",
                    new[] { nameof(StartDate), nameof(EndDate) }
                );
            }
        }
    }
}
