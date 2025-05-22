namespace IkıncielKitapApi.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<Book>? Books { get; set; }
    }

}
