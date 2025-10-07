namespace etickets_aspnet_api.Areas.Admin.DTOs.Response
{
    public class ActorsResponse
    {
        public int Id { get; set; }
        [Required]
        [MinLength(3)]
        [MaxLength(10)]

        public string FirstName { get; set; } = null!;

        [Required]
        [MinLength(3)]
        [MaxLength(10)]
        public string LastName { get; set; } = null!;

        public string? Bio { get; set; }

        public string? ProfilePicture { get; set; }

        public string? News { get; set; }
        public List<MoviesResponse> Movies { get; set; }
        //public List<Movie>? Movies { get; set; }
    }
}
