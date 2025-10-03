namespace etickets_aspnet_api.Models
{
    [PrimaryKey(nameof(OrderId), nameof(MovieId))]
    public class OrderItems
    {
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int MovieId { get; set; }
        public Movie Movie { get; set; }

        public decimal Price { get; set; }
        public int Count { get; set; }
    }
}
