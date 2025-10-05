namespace etickets_aspnet_api.Areas.Customer.DTOs.Response
{
    public class ActorResponse
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
        
    }
}
