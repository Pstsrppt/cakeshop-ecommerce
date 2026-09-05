using Microsoft.AspNetCore.Mvc;
using Project_CakeShop_66095681.Models.Db;
using Project_CakeShop_66095681.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Globalization; // อย่าลืมเพิ่ม using ตัวนี้ด้านบนสุดครับ
namespace Project_CakeShop_66095681.Controllers
{
    public class StaffController : Controller
    {
        private readonly Csi402dbContext _db;
        private readonly IWebHostEnvironment _environment;

        public StaffController(Csi402dbContext db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }

        // --- 1. Dashboard ---
        public IActionResult Dashboard()
        {
            // 1. ตรวจสอบสิทธิ์การเข้าถึง
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role) || role != "Staff")
            {
                return RedirectToAction("Login", "User");
            }

            // 2. เตรียมข้อมูลวันที่วันนี้
            var today = DateTime.Today;

            // --- ส่วนสรุปตัวเลข (Statistics) ---

            // ออเดอร์ที่รอดำเนินการ (เช่น รอกดส่ง หรือ รอทำเค้ก)
            ViewBag.PendingOrders = _db.Orders.Count(o => o.OrderStatus == "Pending");

            // สินค้าใกล้หมด (สต็อกต่ำกว่า 5 ชิ้น)
            ViewBag.LowStock = _db.Products.Count(p => p.StockQty < 5);

            // ยอดขายเฉพาะ "วันนี้" 
            // หมายเหตุ: กรองเฉพาะออเดอร์ที่สำเร็จ (Completed) หรือตามที่คุณกำหนด
            ViewBag.TodaySales = _db.Orders
                .Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date == today)
                .Sum(o => (double?)o.TotalPrice) ?? 0;

            // จำนวนลูกค้าทั้งหมดที่มีในระบบ
            ViewBag.TotalCustomers = _db.Users.Count(u => u.UserRole == "Customer");

            // --- ส่วนข้อมูลตารางและรายการ ---

            // รายการสินค้าทั้งหมดที่มีสต็อก (ส่งไปแสดงในตารางสินค้าขายดี หรือ POS)
            ViewBag.AllProducts = _db.Products.Where(p => p.StockQty > 0).ToList();

            // ประวัติออเดอร์ล่าสุด 10 รายการ (เรียงจากใหม่ไปเก่า)
            var recentOrders = _db.Orders
                .OrderByDescending(o => o.OrderId)
                .Take(10)
                .ToList();

            return View(recentOrders);
        }
        // --- 2. Checkout + Auto Daily Log ---
        [HttpPost]
        public IActionResult Checkout([FromBody] List<OrderItemRequest> cartItems)
        {
            if (cartItems == null || !cartItems.Any())
                return Json(new { success = false, message = "ไม่มีสินค้าในตะกร้า" });

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    // 1. สร้าง Order หลัก
                    var newOrder = new Order
                    {
                        OrderDate = DateTime.Now,
                        OrderStatus = "Completed",
                        TotalPrice = cartItems.Sum(item => item.Price * item.Qty)
                    };
                    _db.Orders.Add(newOrder);
                    _db.SaveChanges(); // บันทึกเพื่อให้ได้ OrderId มาใช้ต่อ

                    var today = DateOnly.FromDateTime(DateTime.Today);

                    foreach (var item in cartItems)
                    {
                        if (!int.TryParse(item.Id, out int pid)) continue;

                        var product = _db.Products.Find(pid);
                        if (product != null)
                        {
                            // --- เพิ่มส่วนนี้: บันทึกลงตาราง Orderdetails (ที่บิล #3 ขาดไป) ---
                            var orderDetail = new Orderdetail // เช็คตัวสะกดชื่อ Model ในโปรเจกต์คุณด้วยนะครับ
                            {
                                OrderId = newOrder.OrderId, // ใช้ ID จาก Order ที่เพิ่งสร้าง
                                ProductId = pid,
                                Quantity = item.Qty,
                                UnitPrice = (double)item.Price // ใช้ UnitPrice ตามชื่อคอลัมน์ใน DB
                            };
                            _db.Orderdetails.Add(orderDetail);
                            // -------------------------------------------------------

                            // ตัดสต็อกสินค้า
                            int oldQty = product.StockQty;
                            product.StockQty -= item.Qty;

                            // บันทึก StockLog (โค้ดเดิมของคุณ...)
                            var log = _db.StockLogs.FirstOrDefault(l => l.ProductId == pid && l.LogDate == today);
                            if (log == null)
                            {
                                _db.StockLogs.Add(new StockLog
                                {
                                    ProductId = pid,
                                    LogDate = today,
                                    OpeningQty = oldQty,
                                    SoldQty = item.Qty,
                                    ClosingQty = product.StockQty
                                });
                            }
                            else
                            {
                                log.SoldQty += item.Qty;
                                log.ClosingQty = product.StockQty;
                            }
                        }
                    }

                    _db.SaveChanges();
                    transaction.Commit();
                    return Json(new { success = true, message = "ชำระเงินเรียบร้อย" });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { success = false, message = "เกิดข้อผิดพลาด: " + ex.Message });
                }
            }
        }

        // --- 3. Inventory ---
        public IActionResult Inventory()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Staff") return RedirectToAction("Login", "User");

            var products = _db.Products.OrderBy(p => p.CakeName).ToList();
            return View(products);
        }

        // --- 4. Update Stock + Log ---
        [HttpPost]
        public IActionResult UpdateStock(int id, int newQty)
        {
            var product = _db.Products.Find(id);
            if (product != null)
            {
                int oldQty = product.StockQty;
                int diff = newQty - oldQty;
                product.StockQty = newQty;

                var today = DateOnly.FromDateTime(DateTime.Today);
                var log = _db.StockLogs.FirstOrDefault(l => l.ProductId == id && l.LogDate == today);

                if (log == null)
                {
                    _db.StockLogs.Add(new StockLog
                    {
                        ProductId = id,
                        LogDate = today,
                        OpeningQty = oldQty,
                        AddedQty = diff > 0 ? diff : 0,
                        SoldQty = 0,
                        ClosingQty = newQty
                    });
                }
                else
                {
                    if (diff > 0) log.AddedQty += diff;
                    log.ClosingQty = newQty;
                }

                _db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // --- 5. Add Product ---
        [HttpPost]
        public async Task<IActionResult> AddProduct(Product model, IFormFile imageFile)
        {
            try
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string fileName = "cake_" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(imageFile.FileName);
                    string uploadDir = Path.Combine(_environment.WebRootPath, "images", "products");
                    if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                    string filePath = Path.Combine(uploadDir, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    model.ProductImage = "/images/products/" + fileName;
                }

                _db.Products.Add(model);
                await _db.SaveChangesAsync();
                return RedirectToAction("Inventory");
            }
            catch (Exception ex)
            {
                return BadRequest("Error: " + ex.Message);
            }
        }
        [HttpPost]
        public IActionResult DeleteOrder(int id)
        {
            try
            {
                // ค้นหาบิล
                var order = _db.Orders.Find(id);
                if (order == null) return Json(new { success = false, message = "ไม่พบข้อมูลบิล" });

                // ลบรายละเอียดบิลก่อน (ถ้ามี) เพื่อป้องกัน Error Foreign Key
                var details = _db.Orderdetails.Where(d => d.OrderId == id);
                _db.Orderdetails.RemoveRange(details);

                // ลบบิลหลัก
                _db.Orders.Remove(order);
                _db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // --- 6. Delete ---
        [HttpPost]
        public IActionResult DeleteProduct(int id)
        {
            var product = _db.Products.Find(id);
            if (product == null)
                return Json(new { success = false, message = "ไม่พบสินค้า" });

            // ✅ เช็คก่อนว่ามีออเดอร์ค้างอยู่ไหม
            bool hasOrders = _db.Orderdetails.Any(d => d.ProductId == id);
            if (hasOrders)
                return Json(new { success = false, message = "ไม่สามารถลบได้ เพราะสินค้านี้มีประวัติออเดอร์อยู่" });

            _db.Products.Remove(product);
            _db.SaveChanges();
            return Json(new { success = true });
        }

        // --- 7. Stock Report ---
        public IActionResult StockReport(string searchDate) // เปลี่ยนมารับเป็น string ก่อนเพื่อดักปี
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Staff") return RedirectToAction("Login", "User");

            DateTime targetDateTime;

            // ตรวจสอบว่าส่งค่าวันที่มาไหม ถ้าไม่ส่งให้ใช้ วันนี้
            if (string.IsNullOrEmpty(searchDate))
            {
                targetDateTime = DateTime.Today;
            }
            else
            {
                // บังคับ Parse ให้เป็น ค.ศ. เท่านั้น ไม่ว่าเครื่องจะเป็น พ.ศ. หรือไม่
                DateTime.TryParse(searchDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out targetDateTime);
            }

            // ถ้าปีมันหลุดไปไกลเกิน (เช่น > 2500) ให้ลบออก 543 เพื่อดึงกลับมาเป็น ค.ศ.
            if (targetDateTime.Year > 2500)
            {
                targetDateTime = targetDateTime.AddYears(-543);
            }

            var targetDateOnly = DateOnly.FromDateTime(targetDateTime);

            var logs = _db.StockLogs
                .Include(l => l.Product)
                .Where(l => l.LogDate == targetDateOnly)
                .ToList();

            // ส่งค่ากลับไปหา View แบบบังคับฟอร์แมต ค.ศ. (yyyy-MM-dd)
            ViewBag.SelectedDate = targetDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            ViewBag.RawDate = targetDateTime;

            return View(logs);
        }
        public IActionResult Logout()
        {
            // ล้างข้อมูลทั้งหมดใน Session
            HttpContext.Session.Clear();

            // หรือระบุลบทีละตัวเพื่อความชัวร์
            // HttpContext.Session.Remove("UserName");
            // HttpContext.Session.Remove("UserRole");

            // ส่งกลับไปหน้า Welcome ของ Home
            return RedirectToAction("Welcome", "Home");
        }
        // --- ส่วนที่ 1: หน้าแสดงประวัติการขาย ---
        public IActionResult ManageOrders(string filterDate, int? filterMonth)
        {
            var query = _db.Orders.AsQueryable();
            DateTime targetDate = DateTime.MinValue;

            // 1. จัดการเรื่องวันที่ (Filter Date)
            if (!string.IsNullOrEmpty(filterDate))
            {
                // บังคับอ่านค่าเป็น ค.ศ. (InvariantCulture)
                DateTime.TryParse(filterDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out targetDate);

                // ดักจับถ้าปีที่ส่งมาเป็น พ.ศ. ให้แปลงกลับเป็น ค.ศ.
                if (targetDate.Year > 2500) targetDate = targetDate.AddYears(-543);

                query = query.Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date == targetDate.Date);

                // ส่งค่ากลับไปโชว์ในช่อง input (ต้องเป็นฟอร์แมต yyyy-MM-dd)
                ViewBag.SelectedDate = targetDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            // 2. จัดการเรื่องเดือน (Filter Month) - จะทำงานเฉพาะถ้าไม่ได้เลือกวันที่เจาะจง
            if (filterMonth.HasValue && string.IsNullOrEmpty(filterDate))
            {
                int currentYear = DateTime.Now.Year;
                // ถ้าเครื่องเป็น พ.ศ. ต้องปรับ currentYear ให้เป็น ค.ศ. ก่อนเปรียบเทียบกับ DB
                if (currentYear > 2500) currentYear -= 543;

                query = query.Where(o => o.OrderDate.HasValue &&
                                        o.OrderDate.Value.Month == filterMonth.Value &&
                                        o.OrderDate.Value.Year == currentYear);
                ViewBag.SelectedMonth = filterMonth.Value;
            }

            var orders = query.OrderByDescending(o => o.OrderDate).ToList();
            ViewBag.TotalSales = orders.Sum(x => x.TotalPrice);

            return View(orders);
        }

        // --- ส่วนที่ 2: แก้ไขเส้นแดงใน GetSalesForCalendar (บรรทัด 276) ---
        public IActionResult GetSalesForCalendar()
        {
            var salesData = _db.Orders
                .Where(o => o.OrderDate.HasValue)
                .GroupBy(o => o.OrderDate.Value.Date)
                .Select(g => new
                {
                    title = "ยอด: ฿" + g.Sum(x => x.TotalPrice).ToString("N0"),
                    start = g.Key.ToString("yyyy-MM-dd"),
                    color = "#6366f1"
                }).ToList();

            return Json(salesData);
        }
        [HttpGet]
        public IActionResult GetOrderDetails(int id)
        {
            var order = _db.Orders
                .Include(o => o.Orderdetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null) return NotFound();

            var details = order.Orderdetails.Select(d => new
            {
                productName = d.Product?.CakeName ?? "ไม่ระบุชื่อสินค้า",
                quantity = d.Quantity,
                price = d.UnitPrice // แก้ไขจาก Price เป็น UnitPrice ตามฐานข้อมูลของคุณ
            }).ToList();

            return Json(details);
        }
        [HttpPost]
        public async Task<IActionResult> EditProduct(int ProductId, string CakeName, double Price,
    double DiscountAmount, int StockQty, string? ExpiryDate, IFormFile? imageFile)
        {
            var product = _db.Products.Find(ProductId);
            if (product == null) return Json(new { success = false, message = "ไม่พบสินค้า" });

            product.CakeName = CakeName;
            product.Price = Price;
            product.DiscountAmount = DiscountAmount;
            product.StockQty = StockQty;

            if (!string.IsNullOrEmpty(ExpiryDate))
                product.ExpiryDate = DateOnly.Parse(ExpiryDate);

            if (imageFile != null && imageFile.Length > 0)
            {
                string fileName = "cake_" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(imageFile.FileName);
                string uploadDir = Path.Combine(_environment.WebRootPath, "images", "products");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                string filePath = Path.Combine(uploadDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                product.ProductImage = "/images/products/" + fileName;
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }
        // --- อัปเดตสถานะออเดอร์ (Staff) ---
        [HttpPost]
        public IActionResult UpdateOrderStatus(int id, string status)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Staff") return Json(new { success = false, message = "ไม่มีสิทธิ์" });

            var order = _db.Orders.Find(id);
            if (order == null) return Json(new { success = false, message = "ไม่พบออเดอร์" });

            order.OrderStatus = status;
            _db.SaveChanges();
            return Json(new { success = true, message = $"อัปเดตสถานะเป็น {status} เรียบร้อย" });
        }

        // --- ตรวจสอบ/ยืนยันการชำระเงิน (Staff) ---
        [HttpPost]
        public IActionResult VerifyPayment(int id, string paymentStatus)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Staff") return Json(new { success = false, message = "ไม่มีสิทธิ์" });

            var order = _db.Orders.Find(id);
            if (order == null) return Json(new { success = false, message = "ไม่พบออเดอร์" });

            order.PaymentStatus = paymentStatus;
            // ถ้ายืนยันการชำระเงินแล้ว ให้เปลี่ยนสถานะออเดอร์เป็น Processing
            if (paymentStatus == "Paid" && order.OrderStatus == "Pending")
            {
                order.OrderStatus = "Processing";
            }
            _db.SaveChanges();
            return Json(new { success = true, message = $"อัปเดตการชำระเงินเป็น {paymentStatus} เรียบร้อย" });
        }

        // 1. ดึงรายการโปรโมชัน
        public IActionResult GetPromotions()
        {
            var promos = _db.Promotions.OrderByDescending(p => p.EndDate).ToList();
            return Json(promos);
        }

        // 2. เพิ่มโปรโมชัน
        [HttpPost]
        public IActionResult AddPromotion(string promoCode, string? description, int discountPercent, string? endDate)
        {
            if (_db.Promotions.Any(p => p.PromoCode == promoCode))
                return Json(new { success = false, message = "รหัสนี้มีอยู่แล้ว" });

            _db.Promotions.Add(new Promotion
            {
                PromoCode = promoCode,
                Description = description,
                DiscountPercent = discountPercent,
                EndDate = string.IsNullOrEmpty(endDate) ? null : DateOnly.Parse(endDate)
            });
            _db.SaveChanges();
            return Json(new { success = true });
        }

        // 3. ลบโปรโมชัน
        [HttpPost]
        public IActionResult DeletePromotion(string promoCode)
        {
            var promo = _db.Promotions.Find(promoCode);
            if (promo == null) return Json(new { success = false });
            _db.Promotions.Remove(promo);
            _db.SaveChanges();
            return Json(new { success = true });
        }

        // 4. Export CSV
        public IActionResult GetTodayOrdersForExport()
        {
            var today = DateTime.Today;
            var orders = _db.Orders
                .Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date == today)
                .OrderByDescending(o => o.OrderId)
                .Select(o => new
                {
                    o.OrderId,
                    orderDate = o.OrderDate.Value.ToString("dd/MM/yyyy HH:mm"),
                    o.TotalPrice,
                    o.OrderStatus
                }).ToList();
            return Json(orders);
        }
    }
}