namespace Project_CakeShop_66095681.Models
{
    public class LoginViewModel
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";

        // เก็บค่าที่แสดงหลัง Login สำเร็จ
        public string? WelcomeMessage { get; set; }
        public string? UserRole { get; set; }
    }
}