using System;
using System.Collections.Generic;

namespace Project_CakeShop_66095681.Models.Db;

public partial class Orderdetail
{
    public int DetailId { get; set; }

    public int? OrderId { get; set; }

    public int? ProductId { get; set; }

    public int Quantity { get; set; }

    public double UnitPrice { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Product? Product { get; set; }
}
