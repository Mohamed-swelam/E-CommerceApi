namespace Core.Models
{
    public class Shipping
    {
        public int Id { get; set; }

        public string TrackingNumber { get; set; }

        public string Carrier { get; set; }

        public DateTime? ShippedDate { get; set; }

        public DateTime? DeliveredDate { get; set; }

        public int OrderId { get; set; }

        public Order? Order { get; set; }
    }
}
