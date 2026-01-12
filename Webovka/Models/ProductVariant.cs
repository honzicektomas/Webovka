using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Webovka.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [Required]
        public string Color { get; set; } 

        public string ColorHex { get; set; } 

        [Required]
        public string Size { get; set; }

        public int StockQuantity { get; set; }
    }
}