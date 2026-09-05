using Microsoft.EntityFrameworkCore;
using Project_CakeShop_66095681.Models.Db; // เช็คชื่อ Namespace ให้ตรงกับโปรเจคคุณนะครับ

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();   

// เพิ่มส่วนนี้เข้าไปครับ
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found. Set it in appsettings.Development.json or user secrets.");

builder.Services.AddDbContext<Csi402dbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles(); // เปลี่ยนจาก MapStaticAssets เป็นมาตรฐานก่อนเพื่อเช็ค

app.UseRouting();

// ย้าย UseSession มาไว้ตรงนี้ (หลัง Routing ก่อน Authorization)
app.UseSession(); 

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Welcome}/{id?}");

app.Run();
