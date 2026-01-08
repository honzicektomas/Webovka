using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Webovka.Models
{
    [Table("ProductVariants")]
    public class ProductVariant
    {
        public int Id { get; set; }

        public string Color { get; set; } 
        public string Size { get; set; } 
        public int StockQuantity { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}