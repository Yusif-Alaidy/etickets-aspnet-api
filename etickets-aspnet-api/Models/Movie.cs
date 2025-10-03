namespace etickets_aspnet_api.Models
{
    public partial class Movie
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
        public Cinema? Cinema { get; set; }
        [Required]
        public int CategoryId { get; set; }
        [Required]
        public Category? Category { get; set; }
        //public List<int> ActorIds { get; set; } = new List<int>();
        public List<Actor>? Actors { get; set; }

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