using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_CakeShop_66095681.Models.Db;

namespace Project_CakeShop_66095681.Controllers
{
    public class AdminController : Controller
    {
        private readonly Csi402dbContext _db;
        private readonly IWebHostEnvironment _webHost;

        public AdminController(Csi402dbContext db, IWebHostEnvironment webHost)
        {
            _db = db;
            _webHost = webHost;
        }

        // Helper Method สำหรับเช็กสิทธิ์
        private string? GetUserRole() => HttpContext.Session.GetString("UserRole");

        // --- 1. Dashboard ---
        public IActionResult Dashboard()
        {
            if (GetUserRole() != "Admin") return RedirectToAction("Welcome", "Home");

            ViewBag.TotalSales = _db.Orders.Any() ? _db.Orders.Sum(o => o.TotalPrice) : 0;
            ViewBag.OrderCount = _db.Orders.Count();
            ViewBag.UserCount = _db.Users.Count(u => u.UserRole == "Customer");
            ViewBag.ProductCount = _db.Products.Count();

            var recentOrders = _db.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToList();

            return View(recentOrders);
        }

        // --- 2. การจัดการสินค้า (Inventory) ---
        [Route("Admin/ManageProduct")]
        public IActionResult ManageProduct()
        {
            var role = GetUserRole();
            if (role != "Admin" && role != "Staff") return RedirectToAction("Welcome", "Home");

            var products = _db.Products.ToList();
            return View("ManageProduct", products);
        }

        // 1. หน้าแสดงฟอร์ม Create
        public IActionResult CreateProduct()
        {
            if (GetUserRole() != "Admin") return RedirectToAction("Welcome", "Home");
            return View();
        }

        // 2. ฟังก์ชันรับค่าจากฟอร์ม Create พร้อมจัดการรูปภาพ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product, IFormFile? uploadImage)
        {
            if (GetUserRole() != "Admin") return RedirectToAction("Welcome", "Home");

            if (ModelState.IsValid)
            {
                if (uploadImage != null && uploadImage.Length > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadImage.FileName);
                    string uploadDir = Path.Combine(_webHost.WebRootPath, "images", "products");
                    if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                    string filePath = Path.Combine(uploadDir, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadImage.CopyToAsync(stream);
                    }
                    product.ProductImage = "/images/products/" + fileName;
                }
                else
                {
                    product.ProductImage = "/images/no-image.jpg";
                }

                _db.Products.Add(product);
                await _db.SaveChangesAsync();
                return RedirectToAction("ManageProduct");
            }
            return View(product);
        }

        // --- ยอดขาย 7 วันสำหรับ Chart ---
        public IActionResult GetSalesLast7Days()
        {
            var today = DateTime.Today;
            var result = Enumerable.Range(0, 7)
                .Select(i => today.AddDays(-6 + i))
                .Select(date => new
                {
                    label = date.ToString("dd/MM"),
                    total = _db.Orders
                        .Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date == date)
                        .Sum(o => (double?)o.TotalPrice) ?? 0
                }).ToList();

            return Json(result);
        }

        // 3. หน้าแสดงฟอร์ม Edit
        public IActionResult EditProduct(int id)
        {
            if (GetUserRole() != "Admin") return RedirectToAction("Welcome", "Home");
            var product = _db.Products.Find(id);
            if (product == null) return NotFound();
            return View(product);
        }

        // 4. ฟังก์ชันบันทึกการแก้ไข Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(Product model, IFormFile? imageFile)
        {
            if (GetUserRole() != "Admin") return RedirectToAction("Welcome", "Home");

            var existingProduct = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == model.ProductId);
            if (existingProduct == null) return NotFound();

            if (imageFile != null && imageFile.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string uploadDir = Path.Combine(_webHost.WebRootPath, "images", "products");

                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                string filePath = Path.Combine(uploadDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                model.ProductImage = "/images/products/" + fileName;
            }
            else
            {
                model.ProductImage = existingProduct.ProductImage;
            }

            _db.Update(model);
            await _db.SaveChangesAsync();
            return RedirectToAction("ManageProduct");
        }

        // 5. ฟังก์ชันลบสินค้า
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteProduct(int id)
        {
            if (GetUserRole() != "Admin") return RedirectToAction("Welcome", "Home");

            bool hasOrders = _db.Orderdetails.Any(d => d.ProductId == id);
            if (hasOrders)
            {
                TempData["Error"] = "ไม่สามารถลบได้ เพราะสินค้านี้มีประวัติออเดอร์อยู่";
                return RedirectToAction("ManageProduct");
            }

            var product = _db.Products.Find(id);
            if (product != null)
            {
                _db.Products.Remove(product);
                _db.SaveChanges();
            }
            return RedirectToAction("ManageProduct");
        }

        // --- 3. การจัดการออเดอร์ ---
        public IActionResult ManageOrders(string? filterDate)
        {
            var role = GetUserRole();
            if (role != "Admin" && role != "Staff") return RedirectToAction("Welcome", "Home");

            var query = _db.Orders
                .Include(o => o.User)
                .Include(o => o.Orderdetails)
                    .ThenInclude(d => d.Product)
                .AsQueryable();

            // กรองตามวันที่ถ้ามีการเลือก
            if (!string.IsNullOrEmpty(filterDate) && DateOnly.TryParse(filterDate, out var parsedDate))
            {
                query = query.Where(o => o.OrderDate.HasValue &&
                    o.OrderDate.Value.Year == parsedDate.Year &&
                    o.OrderDate.Value.Month == parsedDate.Month &&
                    o.OrderDate.Value.Day == parsedDate.Day);
            }

            var orders = query.OrderByDescending(o => o.OrderDate).ToList();

            ViewBag.TotalSales = orders.Sum(o => o.TotalPrice);
            ViewBag.SelectedDate = filterDate ?? "";

            return View(orders);
        }

        // --- อัปเดตสถานะออเดอร์ ---
        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, string newStatus)
        {
            var role = GetUserRole();
            if (role != "Admin" && role != "Staff") return Unauthorized();

            var order = _db.Orders.Find(orderId);
            if (order != null)
            {
                order.OrderStatus = newStatus;
                _db.SaveChanges();
            }
            return RedirectToAction("ManageOrders");
        }

        // --- 4. การจัดการผู้ใช้ ---
        public IActionResult ManageUsers()
        {
            if (GetUserRole() != "Admin") return RedirectToAction("Welcome", "Home");
            var users = _db.Users.ToList();
            return View(users);
        }

        [HttpPost]
        public IActionResult UpdateRole(int userId, string newRole)
        {
            if (GetUserRole() != "Admin") return Unauthorized();

            var user = _db.Users.Find(userId);
            if (user != null)
            {
                user.UserRole = newRole;
                _db.SaveChanges();
            }
            return RedirectToAction("ManageUsers");
        }
    }
}
