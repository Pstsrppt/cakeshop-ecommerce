using System;
using System.Collections.Generic;

namespace Project_CakeShop_66095681.Models.Db;

public partial class Promotion
{
    public string PromoCode { get; set; } = null!;

    public string? Description { get; set; }

    public int? DiscountPercent { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }
}
