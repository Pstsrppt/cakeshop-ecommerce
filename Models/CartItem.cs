namespace Project_CakeShop_66095681.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string CakeName { get; set; } = "";
        public double Price { get; set; }
        public int Quantity { get; set; }
        public string? ProductImage { get; set; } // ตรวจสอบชื่อนี้ให้ตรงกับใน View
        public double Total => Price * Quantity;
    }
}