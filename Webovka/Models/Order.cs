using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
// using System.ComponentModel.DataAnnotations; <--- Tohle smažte nebo zakomentujte

namespace Webovka.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        public DateTime OrderDate { get; set; }

        public string State { get; set; }

        public decimal TotalPrice { get; set; }

        // --- ZDE JSME SMAZALI [Required], ABY MOHL EXISTOVAT KOŠÍK BEZ JMÉNA ---
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }

        public List<OrderItem> OrderItems { get; set; }
    }
}