using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BenchDemoOrderFunction.Model
{
    public class CreateOrderRequest
    {
        [Required]
        public Guid? CustomerId { get; set; }


        [Required]
        public Guid? ProductId { get; set; }

        [Required]
        [Range(1,1000)]
        public int? Quantity { get; set; }

        [Required]
        [RegularExpression("^(Pending|Processing|Completed|Cancelled)")]
        public string? Status { get; set; }

        [Required]
        [EmailAddress]
        public string? EmailAddress { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Product price must be a positive value.")]
        public double? ProductPrice { get; set; }


    }
}
