using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_CakeShop_66095681.Models;
using Project_CakeShop_66095681.Models.Db;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Project_CakeShop_66095681.Controllers
{
    public class UserController : Controller
    {
        private readonly Csi402dbContext _db;

        public UserController(Csi402dbContext db)
        {
            _db = db;
        }

        // --- Helper Methods ---
        private List<CartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cartJson)) return new List<CartItem>();
            return JsonSerializer.Deserialize<List<CartItem>>(cartJson)!;
        }

        private void SaveCart(List<CartItem> cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("Cart", cartJson);
        }

        // --- 1. ระบบสมาชิก ---
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_db.Users.Any(u => u.Username == model.Username))
                {
                    ViewBag.Error = "ชื่อผู้ใช้นี้ถูกใช้งานแล้ว กรุณาเลือกชื่อผู้ใช้อื่น";
                    return View(model);
                }

                var newUser = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Username = model.Username,
                    Password = model.Password,
                    UserRole = "Customer"
                };
                _db.Users.Add(newUser);
                _db.SaveChanges();
                return RedirectToAction("Login");
            }
            return View(model);
        }

        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string Username, string Password)
        {
            var user = _db.Users.FirstOrDefault(u => u.Username == Username && u.Password == Password);
            if (user != null)
            {
                HttpContext.Session.SetString("UserName", user.FullName);
                HttpContext.Session.SetString("UserRole", user.UserRole ?? "Customer");
                HttpContext.Session.SetInt32("UserId", user.UserId);

                if (user.UserRole == "Admin") return RedirectToAction("Dashboard", "Admin");
                if (user.UserRole == "Staff") return RedirectToAction("Dashboard", "Staff");

                return RedirectToAction("Welcome", "Home");
            }
            ViewBag.Error = "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง!";
            return View();
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete(".AspNetCore.Session");
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            return RedirectToAction("Welcome", "Home");
        }

        // --- 2. ระบบหน้าร้านและตะกร้าสินค้า ---
        public IActionResult ProductCustomer()
        {
            var products = _db.Products.ToList();
            return View(products);
        }

        // หน้ารายละเอียดสินค้า (สต็อก, วันหมดอายุ)
        public IActionResult ProductDetail(int id)
        {
            var product = _db.Products.Find(id);
            if (product == null) return RedirectToAction("ProductCustomer");
            return View(product);
        }

        public IActionResult Cart()
        {
            var cart = GetCart();
            return View(cart);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId)
        {
            var product = _db.Products.Find(productId);
            if (product == null) return NotFound();

            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.ProductId,
                    CakeName = product.CakeName,
                    Price = product.Price - (product.DiscountAmount ?? 0),
                    Quantity = 1,
                    ProductImage = product.ProductImage
                });
            }

            SaveCart(cart);
            return RedirectToAction("ProductCustomer");
        }

        public IActionResult IncreaseQuantity(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == id);
            if (item != null) { item.Quantity++; SaveCart(cart); }
            return RedirectToAction("Cart");
        }

        public IActionResult DecreaseQuantity(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == id);
            if (item != null)
            {
                if (item.Quantity > 1) item.Quantity--;
                else cart.Remove(item);
                SaveCart(cart);
            }
            return RedirectToAction("Cart");
        }

        public IActionResult RemoveFromCart(int id)
        {
            var cart = GetCart();
            var itemToRemove = cart.FirstOrDefault(c => c.ProductId == id);
            if (itemToRemove != null) { cart.Remove(itemToRemove); SaveCart(cart); }
            return RedirectToAction("Cart");
        }

        // --- 3. ระบบโปรโมโค้ด ---
        // คำนวณส่วนลดตาม Logic โปรโมชัน 5 แบบ
        private (double discount, string message, bool success) CalculatePromo(string promoCode, List<CartItem> cart)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var promo = _db.Promotions.FirstOrDefault(p =>
                p.PromoCode == promoCode &&
                (p.StartDate == null || p.StartDate <= today) &&
                (p.EndDate == null || p.EndDate >= today));

            if (promo == null)
                return (0, "❌ ไม่พบโปรโมโค้ด หรือโค้ดหมดอายุแล้ว", false);

            double subTotal = cart.Sum(x => x.Total);
            int totalQty = cart.Sum(x => x.Quantity);

            // ✅ Logic โปรโมชัน 5 แบบตาม Requirement
            double discount = 0;
            string msg = "";

            switch (promoCode.ToUpper())
            {
                // โปรโมชัน 1: ลูกค้าใหม่ สั่งครั้งแรก ลด 20 บาท (เมื่อซื้อครบ 100 บาท)
                case "NEWCUSTOMER":
                case "WELCOME20":
                    var userId = HttpContext.Session.GetInt32("UserId");
                    if (userId == null) { return (0, "❌ กรุณาเข้าสู่ระบบก่อน", false); }
                    var orderCount = _db.Orders.Count(o => o.UserId == userId);
                    if (orderCount > 0) { return (0, "❌ โปรโมชันนี้สำหรับลูกค้าใหม่เท่านั้น", false); }
                    if (subTotal < 100) { return (0, "❌ ต้องสั่งซื้อขั้นต่ำ 100 บาท", false); }
                    discount = 20;
                    msg = "✅ ส่วนลดลูกค้าใหม่ 20 บาท";
                    break;

                // โปรโมชัน 2: ซื้อ 6 แถม 1 (ลดราคาสินค้าที่ถูกที่สุด 1 ชิ้น)
                case "BUY6GET1":
                    if (totalQty < 6) { return (0, $"❌ ต้องซื้ออย่างน้อย 6 ชิ้น (ตอนนี้มี {totalQty} ชิ้น)", false); }
                    var cheapest = cart.Min(x => x.Price);
                    discount = cheapest;
                    msg = $"✅ ซื้อ 6 แถม 1 (ฟรี 1 ชิ้นราคา ฿{cheapest:N0})";
                    break;

                // โปรโมชัน 3: ซื้อ 10 ชิ้นขึ้นไป ลด 10%
                case "BUY10":
                case "BULK10":
                    if (totalQty < 10) { return (0, $"❌ ต้องซื้ออย่างน้อย 10 ชิ้น (ตอนนี้มี {totalQty} ชิ้น)", false); }
                    discount = subTotal * 0.10;
                    msg = $"✅ ลด 10% เมื่อซื้อครบ 10 ชิ้น (-฿{discount:N0})";
                    break;

                // โปรโมชัน 4: ส่งฟรีเมื่อสั่ง 299 บาทขึ้นไป (ลด shipping 50 บาท)
                case "FREESHIP":
                case "SHIP299":
                    if (subTotal < 299) { return (0, $"❌ ต้องสั่งซื้อขั้นต่ำ 299 บาท (ตอนนี้ ฿{subTotal:N0})", false); }
                    discount = 50; // shipping fee = 50 บาท
                    msg = "✅ ส่งฟรี! (ลดค่าส่ง 50 บาท)";
                    break;

                // โปรโมชัน 5: Flash Sale ลด 20%
                case "FLASH20":
                case "FLASHSALE":
                    discount = subTotal * 0.20;
                    msg = $"✅ Flash Sale! ลด 20% (-฿{discount:N0})";
                    break;

                // โปรโมชันทั่วไป (ใช้ DiscountPercent จาก DB)
                default:
                    if (promo.DiscountPercent.HasValue && promo.DiscountPercent > 0)
                    {
                        discount = subTotal * (promo.DiscountPercent.Value / 100.0);
                        msg = $"✅ ลด {promo.DiscountPercent}% (-฿{discount:N0})";
                    }
                    else
                    {
                        return (0, "❌ โปรโมโค้ดนี้ไม่มีส่วนลด", false);
                    }
                    break;
            }

            return (discount, msg, true);
        }

        [HttpPost]
        public IActionResult ApplyPromo(string promoCode)
        {
            var cart = GetCart();
            if (cart == null || !cart.Any())
            {
                TempData["PromoError"] = "❌ ตะกร้าของคุณว่างเปล่า";
                return RedirectToAction("Cart");
            }

            var (discount, message, success) = CalculatePromo(promoCode?.Trim().ToUpper() ?? "", cart);

            if (success)
            {
                HttpContext.Session.SetString("PromoCode", promoCode!.Trim().ToUpper());
                HttpContext.Session.SetString("PromoDiscount", discount.ToString());
                HttpContext.Session.SetString("PromoMessage", message);
                TempData["PromoSuccess"] = message;
            }
            else
            {
                HttpContext.Session.Remove("PromoCode");
                HttpContext.Session.Remove("PromoDiscount");
                HttpContext.Session.Remove("PromoMessage");
                TempData["PromoError"] = message;
            }

            return RedirectToAction("Cart");
        }

        [HttpGet]
        public IActionResult RemovePromo()
        {
            HttpContext.Session.Remove("PromoCode");
            HttpContext.Session.Remove("PromoDiscount");
            HttpContext.Session.Remove("PromoMessage");
            return RedirectToAction("Cart");
        }

        // --- 4. ระบบชำระเงิน (Checkout) ---
        [HttpPost]
        public async Task<IActionResult> Checkout(string? shippingAddress)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var cart = GetCart();
            if (cart == null || cart.Count == 0) return RedirectToAction("ProductCustomer");

            double subTotal = cart.Sum(x => x.Total);
            int totalQty = cart.Sum(x => x.Quantity);

            // ✅ โปรอัตโนมัติ (ทำงานพร้อมกันได้)
            double autoBuy6 = totalQty >= 6 ? cart.Min(x => x.Price) : 0;
            double autoBuy10 = totalQty >= 10 ? subTotal * 0.10 : 0;

            // ✅ โปรโมโค้ด (Flash Sale / NEWCUSTOMER)
            double promoDiscount = 0;
            var promoDiscStr = HttpContext.Session.GetString("PromoDiscount");
            if (!string.IsNullOrEmpty(promoDiscStr))
                double.TryParse(promoDiscStr, out promoDiscount);

            double grandTotal = subTotal - autoBuy6 - autoBuy10 - promoDiscount;
            if (grandTotal < 0) grandTotal = 0;

            var newOrder = new Order
            {
                UserId = userId.Value,
                OrderDate = DateTime.Now,
                TotalPrice = grandTotal,
                OrderStatus = "Pending",
                ShippingAddress = shippingAddress,
                PaymentStatus = "Unpaid"
            };

            _db.Orders.Add(newOrder);
            await _db.SaveChangesAsync();

            foreach (var item in cart)
            {
                var detail = new Orderdetail
                {
                    OrderId = newOrder.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                };
                _db.Orderdetails.Add(detail);

                var product = await _db.Products.FindAsync(item.ProductId);
                if (product != null) product.StockQty -= item.Quantity;
            }

            await _db.SaveChangesAsync();

            // เคลียร์ตะกร้าและโปรโม
            HttpContext.Session.Remove("Cart");
            HttpContext.Session.Remove("PromoCode");
            HttpContext.Session.Remove("PromoDiscount");
            HttpContext.Session.Remove("PromoMessage");

            return View("OrderSuccess", newOrder.OrderId);
        }

        // --- 5. ประวัติการสั่งซื้อ ---
        public IActionResult OrderHistory()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var orders = _db.Orders
                .Include(o => o.Orderdetails)
                    .ThenInclude(d => d.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // --- 6. โปรไฟล์ ---
        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");
            var user = _db.Users.Find(userId);
            return View(user);
        }
        [HttpPost]
        public IActionResult UpdateProfile(int userId, string fullName, string? email)
        {
            var user = _db.Users.Find(userId);
            if (user == null) return RedirectToAction("Login");

            user.FullName = fullName;
            user.Email = email;
            _db.SaveChanges();

            // อัปเดตชื่อใน Session ด้วย
            HttpContext.Session.SetString("UserName", fullName);

            TempData["Success"] = "บันทึกข้อมูลเรียบร้อยแล้ว";
            return RedirectToAction("Profile");
        }

        // เปลี่ยนรหัสผ่าน
        [HttpPost]
        public IActionResult ChangePassword(int userId, string currentPassword,
                                            string newPassword, string confirmPassword)
        {
            var user = _db.Users.Find(userId);
            if (user == null) return RedirectToAction("Login");

            if (user.Password != currentPassword)
            {
                TempData["Error"] = "รหัสผ่านปัจจุบันไม่ถูกต้อง";
                return RedirectToAction("Profile");
            }

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "รหัสผ่านใหม่ไม่ตรงกัน";
                return RedirectToAction("Profile");
            }

            user.Password = newPassword;
            _db.SaveChanges();

            TempData["Success"] = "เปลี่ยนรหัสผ่านเรียบร้อยแล้ว";
            return RedirectToAction("Profile");
        }
    }
}
