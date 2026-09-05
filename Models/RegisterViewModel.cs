namespace Project_CakeShop_66095681.Models
{
    public class RegisterViewModel
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string ConfirmPassword { get; set; } = ""; // สำหรับเช็กพาสเวิร์ดตรงกันไหม
    }
}