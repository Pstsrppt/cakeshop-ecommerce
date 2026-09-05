# 🎂 CakeShop — ระบบจัดการร้านเค้กออนไลน์

เว็บแอปพลิเคชัน **e-commerce และระบบจัดการสต๊อกสินค้า** สำหรับร้านเค้ก พัฒนาด้วย **ASP.NET Core MVC** และ **MySQL** รองรับการทำงาน 3 บทบาท — ลูกค้า (Customer), พนักงาน (Staff) และผู้ดูแลระบบ (Admin) — แต่ละบทบาทมีหน้าจอและสิทธิ์การใช้งานของตัวเอง ตั้งแต่การเลือกซื้อเค้กไปจนถึงการจัดการสต๊อก โปรโมชั่น และรายงานยอดขาย

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)
![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?logo=dotnet&logoColor=white)
![Entity Framework Core](https://img.shields.io/badge/EF%20Core-9.0-512BD4)
![MySQL](https://img.shields.io/badge/MySQL-9.x-4479A1?logo=mysql&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5-7952B3?logo=bootstrap&logoColor=white)

---

## 📖 ภาพรวมโปรเจกต์

CakeShop คือเว็บแอปที่จำลองการทำงานจริงของร้านเค้ก โดยแบ่งสิทธิ์การใช้งานตามบทบาท (role-based) ครอบคลุมตั้งแต่การสั่งซื้อของลูกค้า การจัดการคำสั่งซื้อของพนักงาน ไปจนถึงการบริหารภาพรวมร้านของผู้ดูแลระบบ ทั้งหมดอยู่ในระบบเดียว

| บทบาท | สิ่งที่ทำได้ |
|------|--------|
| 🛍️ **ลูกค้า (Customer)** | เลือกดูสินค้า จัดการตะกร้าสินค้า ใช้โค้ดส่วนลด ชำระเงิน ดูประวัติการสั่งซื้อ และจัดการโปรไฟล์ |
| 👷 **พนักงาน (Staff)** | จัดการคำสั่งซื้อ ดูแลสต๊อกสินค้า ตรวจสอบการชำระเงิน จัดการโปรโมชั่น และดูปฏิทิน/รายงานยอดขาย |
| 🛠️ **ผู้ดูแลระบบ (Admin)** | จัดการสินค้าแบบครบวงจร (CRUD) จัดการคำสั่งซื้อและผู้ใช้ กำหนดสิทธิ์ผู้ใช้ และดูแดชบอร์ดยอดขาย |

---

## ✨ ฟีเจอร์หลัก

### ฝั่งลูกค้า (Customer)
- หน้ารายการสินค้าพร้อมหน้ารายละเอียดสินค้า
- ตะกร้าสินค้า (เพิ่ม / เพิ่มจำนวน / ลดจำนวน / ลบสินค้า)
- ใส่โค้ดส่วนลดตอนชำระเงิน
- ชำระเงินพร้อมระบุที่อยู่จัดส่ง
- ติดตามประวัติการสั่งซื้อ
- จัดการโปรไฟล์และเปลี่ยนรหัสผ่าน

### ฝั่งพนักงาน (Staff)
- จัดการสต๊อกสินค้าและอัปเดตจำนวนคงเหลือ
- จัดการสถานะคำสั่งซื้อและตรวจสอบการชำระเงิน
- ปฏิทินยอดขายและรายงานสต๊อก
- จัดการโปรโมชั่น (สร้าง / ลบโค้ดส่วนลด)
- ส่งออกข้อมูลคำสั่งซื้อ

### ฝั่งผู้ดูแลระบบ (Admin)
- จัดการสินค้า (เพิ่ม แก้ไข ลบ พร้อมอัปโหลดรูปภาพ)
- จัดการคำสั่งซื้อพร้อมกรองตามวันที่
- จัดการผู้ใช้และกำหนดสิทธิ์
- แดชบอร์ดสรุปยอดขาย 7 วันล่าสุด

### ภาพรวมระบบ
- ระบบยืนยันตัวตนแบบ Session พร้อมจำกัดสิทธิ์ตามบทบาท (Role-based Access Control)
- รองรับอัปโหลดรูปภาพสินค้า
- หน้าจอ Responsive ด้วย Bootstrap 5

---

## 🏗️ เทคโนโลยีที่ใช้

| ส่วนประกอบ | เทคโนโลยี |
|---|---|
| Framework | ASP.NET Core MVC (.NET 10) |
| ORM | Entity Framework Core 9 |
| ฐานข้อมูล | MySQL (ผ่าน [Pomelo.EntityFrameworkCore.MySql](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql)) |
| Frontend | Razor Views, Bootstrap 5, jQuery, jQuery Validation |
| การยืนยันตัวตน | Session-based (`ASP.NET Core Session`) |

---

## 📂 โครงสร้างโปรเจกต์

```
Project_CakeShop_66095681/
├── Controllers/
│   ├── HomeController.cs        # หน้าแรกของเว็บ
│   ├── UserController.cs        # ลูกค้า: ล็อกอิน, สินค้า, ตะกร้า, ชำระเงิน
│   ├── StaffController.cs       # พนักงาน: สต๊อก, คำสั่งซื้อ, โปรโมชั่น
│   └── AdminController.cs       # ผู้ดูแลระบบ: สินค้า, คำสั่งซื้อ, ผู้ใช้, แดชบอร์ด
├── Models/
│   ├── Db/                      # EF Core entities (Product, Order, User, ...)
│   └── ...                      # View models (Login, Register, CartItem, ...)
├── Views/
│   ├── User/ Staff/ Admin/      # หน้าจอแยกตามบทบาท
│   └── Shared/                  # Layout และ navbar ที่ใช้ร่วมกัน
├── wwwroot/                     # ไฟล์ static (css, js, images, client libs)
├── migration_add_columns.sql    # SQL migration เพิ่มคอลัมน์ในตาราง orders
└── Program.cs                   # ตั้งค่าแอปและ Dependency Injection
```

---

## 🗄️ โครงสร้างฐานข้อมูล

ตารางหลักที่จัดการผ่าน EF Core (`Csi402dbContext`):

- **users** — ข้อมูลบัญชีผู้ใช้, รหัสผ่าน, บทบาท (`Customer` / `Staff` / `Admin`)
- **products** — แคตตาล็อกสินค้า: ชื่อ, ราคา, ส่วนลด, จำนวนคงเหลือ, วันหมดอายุ, รูปภาพ
- **orders** — คำสั่งซื้อ: สถานะ, สถานะการชำระเงิน, ที่อยู่จัดส่ง, ยอดรวม
- **orderdetails** — รายการสินค้าย่อยในแต่ละคำสั่งซื้อ
- **promotions** — โค้ดส่วนลดพร้อมเปอร์เซ็นต์ส่วนลด
- **stocklog** — บันทึกความเคลื่อนไหวของสต๊อกสินค้า

---

## 🚀 วิธีติดตั้งและรันโปรเจกต์

### สิ่งที่ต้องมีก่อน
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [MySQL Server](https://dev.mysql.com/downloads/mysql/) (8.0+ / 9.x)

### 1. Clone repository
```bash
git clone https://github.com/Pstsrppt/cakeshop-ecommerce.git
cd cakeshop-ecommerce
```

### 2. สร้างฐานข้อมูล
สร้างฐานข้อมูล MySQL ชื่อ `csi402db` แล้วสร้างตารางตาม EF Core model หรือไฟล์ SQL ที่มี จากนั้นรันคำสั่งนี้เพื่ออัปเดตคอลัมน์เพิ่มเติม:
```bash
mysql -u root -p csi402db < migration_add_columns.sql
```

### 3. ตั้งค่า Connection String
**Connection string จะไม่ถูก commit ขึ้น git** ให้สร้างไฟล์ `appsettings.Development.json` ที่ root ของโปรเจกต์เอง (ไฟล์นี้อยู่ใน `.gitignore` แล้ว):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;user=root;password=YOUR_PASSWORD;database=csi402db;AllowPublicKeyRetrieval=True"
  }
}
```

> **หมายเหตุ:** ต้องใส่ `AllowPublicKeyRetrieval=True` เมื่อเชื่อมต่อ MySQL 8+/9 ที่ใช้ auth plugin เริ่มต้นเป็น `caching_sha2_password` โดยไม่ได้เปิดใช้ SSL ไม่งั้นจะเจอ error "Access denied" ทั้งที่รหัสผ่านถูกต้อง

### 4. Restore และรันโปรเจกต์
```bash
dotnet restore
dotnet run
```

เปิดใช้งานได้ที่ `http://localhost:5000`

---

## 🔒 หมายเหตุด้านความปลอดภัย

- รหัสผ่านฐานข้อมูลถูกแยกออกจากซอร์สโค้ดแล้ว โดยเก็บไว้ใน `appsettings.Development.json` (ไม่ถูก commit) ดูรูปแบบที่ต้องตั้งค่าได้จาก `appsettings.json`
- ⚠️ **รหัสผ่านผู้ใช้ปัจจุบันถูกเก็บและเปรียบเทียบแบบ plain text** (ใน `UserController.Login` / `ChangePassword`) ซึ่งเหมาะสำหรับการส่งงานเรียน/เดโมเท่านั้น **ห้ามนำไปใช้งานจริงหรือใช้กับข้อมูลผู้ใช้จริงโดยไม่เพิ่มระบบ hash รหัสผ่านก่อน (เช่น BCrypt หรือ ASP.NET Core Identity)**

---

## 📜 License

โปรเจกต์นี้จัดทำขึ้นเพื่อการศึกษา (งานรายวิชา CSI402)
