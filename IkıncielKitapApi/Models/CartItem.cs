namespace IkıncielKitapApi.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public int BookId { get; set; }
        public Book? Book { get; set; }

        public DateTime AddedDate { get; set; } = DateTime.Now;

    }
}
