using System;
using System.Collections.Generic;

namespace Project_CakeShop_66095681.Models.Db;

public partial class Product
{
    public int ProductId { get; set; }

    public string CakeName { get; set; } = null!;

    public double Price { get; set; }

    public double? DiscountAmount { get; set; }

    public int StockQty { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public string? ProductImage { get; set; }

    public virtual ICollection<Orderdetail> Orderdetails { get; set; } = new List<Orderdetail>();
}
