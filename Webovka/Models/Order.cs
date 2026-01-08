using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Webovka.Models
{
    [Table("Orders")]
    public class Order
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        public string CustomerName { get; set; }
        [Required]
        public string CustomerEmail { get; set; }
        [Required]
        public string CustomerAddress { get; set; }

        [Required] 
        public string CustomerPhone { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;
        public string State { get; set; } = "Nová";

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}