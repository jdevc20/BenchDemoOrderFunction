using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Text;

namespace BenchDemoOrderFunction.Model
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? ProductId { get; set; }
        public  int? Quantity { get; set; }
        public double? ProductPrice { get; set; }
        public string? Status { get; set; }
        public string? EmailAddress { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
