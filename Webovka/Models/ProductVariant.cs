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
        public string Color { get; set; } // Název barvy (např. "Červená")

        public string ColorHex { get; set; } // NOVÉ: Hex kód (např. "#ff0000")

        [Required]
        public string Size { get; set; }

        public int StockQuantity { get; set; }
    }
}