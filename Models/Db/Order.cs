using System;
using System.Collections.Generic;

namespace Project_CakeShop_66095681.Models.Db;

public partial class Order
{
    public int OrderId { get; set; }

    public int? UserId { get; set; }

    public DateTime? OrderDate { get; set; }

    public double TotalPrice { get; set; }

    public string? OrderStatus { get; set; }

    public string? ShippingAddress { get; set; }

    public string? PaymentStatus { get; set; }

    public virtual ICollection<Orderdetail> Orderdetails { get; set; } = new List<Orderdetail>();

    public virtual User? User { get; set; }
}
