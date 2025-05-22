namespace IkıncielKitapApi.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Author { get; set; } = null!;
        public string Description { get; set; }
        public decimal Price { get; set; }
        public bool IsSold { get; set; }
        public DateTime PostedDate { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
