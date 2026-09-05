using System;
using System.ComponentModel.DataAnnotations; // <--- ต้องมีบรรทัดนี้

namespace Project_CakeShop_66095681.Models.Db;

public partial class StockLog
{
    [Key] // <--- เพิ่มบรรทัดนี้เข้าไปครับ
    public int LogId { get; set; }
    
    public int ProductId { get; set; }
    public DateOnly LogDate { get; set; }
    public int OpeningQty { get; set; }
    public int SoldQty { get; set; }
    public int AddedQty { get; set; }
    public int ClosingQty { get; set; }

    public virtual Product Product { get; set; } = null!;
}