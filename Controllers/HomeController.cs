using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Diagnostics;
using Project_CakeShop_66095681.Models; // เพื่อให้เจอ ErrorViewModel
using Project_CakeShop_66095681.Models.Db; // เพื่อให้เจอ Csi402dbContext

namespace Project_CakeShop_66095681.Controllers;

public class HomeController : Controller
{
    // 1. ต้องประกาศตัวแปรฟิลด์ _db ไว้ตรงนี้ก่อนถึงจะใช้ใน Welcome() ได้
    private readonly Csi402dbContext _db;

    // 2. สร้าง Constructor เพื่อรับค่า DB มาใช้งาน
    public HomeController(Csi402dbContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Welcome()
    {
        var featuredProducts = _db.Products
            .Where(p => p.StockQty > 0)
            .OrderByDescending(p => p.ProductId)
            .Take(4).ToList();

        // ✅ ดึงโปรโมชันที่ยังไม่หมดอายุ
        var today = DateOnly.FromDateTime(DateTime.Today);
        ViewBag.Promotions = _db.Promotions
            .Where(p => p.EndDate == null || p.EndDate >= today)
            .ToList();

        return View(featuredProducts);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        // ใส่ชื่อเต็มไปเลยเพื่อความชัวร์ถ้ายังหา ErrorViewModel ไม่เจอ
        return View(new Project_CakeShop_66095681.Models.ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}