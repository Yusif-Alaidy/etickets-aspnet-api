namespace etickets_aspnet_api.Areas.Customer.DTOs.Response
{
    public class CartResponse
    {
        public string ApplicationUserId { get; set; }
        public int MovieId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }

        //public Movie? Movie { get; set; }

        public int Count { get; set; }
    }
}
