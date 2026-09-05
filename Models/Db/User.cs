using System;
using System.Collections.Generic;

namespace Project_CakeShop_66095681.Models.Db;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? UserRole { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
