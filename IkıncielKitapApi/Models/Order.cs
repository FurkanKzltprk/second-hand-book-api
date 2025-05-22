namespace IkıncielKitapApi.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public Book? Book { get; set; }

        public int BuyerId { get; set; }
        public User? Buyer { get; set; }

        public DateTime OrderDate { get; set; }
    }
}