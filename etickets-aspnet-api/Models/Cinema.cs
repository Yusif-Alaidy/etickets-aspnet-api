namespace etickets_aspnet_api.Models

{
    public partial class Cinema
    {
        public int Id { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(10)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public string? CinemaLogo { get; set; }

        public string? Address { get; set; }
        public List<Movie>? Movies { get; set; }
    }
}