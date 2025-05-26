using System;
using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebInterFace_IkıncielKitapApi.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int BuyerId { get; set; }
        public DateTime OrderDate { get; set; }

        [BindNever]
        public string BookCategory { get; set; }
        [BindNever]
        public string BookTitle { get; set; }
        [BindNever]
        public string BookAuthor { get; set; }
        [BindNever]
        public decimal BookPrice { get; set; }
        [BindNever]
        public string BuyerUsername { get; set; }
        [BindNever]
        public string SellerUsername { get; set; }
    }
}

