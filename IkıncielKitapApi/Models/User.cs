namespace IkıncielKitapApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = "User";
        public ICollection<Book>? Books { get; set; }
        public ICollection<Order>? Orders { get; set; }
    }
}
