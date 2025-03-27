namespace MiniAmazonClone.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public User User { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }

    }
}
