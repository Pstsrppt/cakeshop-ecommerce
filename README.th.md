<!-- Language: [English](README.md) | ภาษาไทย -->

# CakeShop — ระบบ E-Commerce และขายหน้าร้าน (POS)

เว็บแอปพลิเคชัน e-commerce และระบบขายหน้าร้าน (Point of Sale) สำหรับร้านเค้ก พัฒนาด้วย ASP.NET Core MVC, C#, MySQL และ Entity Framework Core รองรับผู้ใช้งาน 3 บทบาท — ลูกค้า, พนักงาน และผู้ดูแลระบบ — ครอบคลุมตั้งแต่การเลือกซื้อสินค้า สั่งซื้อออนไลน์ ไปจนถึงขายหน้าร้านและจัดการสต๊อกสินค้า

**เทคโนโลยีที่ใช้:** ASP.NET Core MVC · C# · MySQL · Entity Framework Core · Razor Views · Bootstrap · Chart.js

---

## ฟีเจอร์หลัก

### ลูกค้า (Customer)
- สมัครสมาชิกและล็อกอิน พร้อม redirect ตามบทบาทผู้ใช้
- เลือกดูสินค้าทั้งหมดในแคตตาล็อก พร้อมแสดงสถานะสินค้าหมด
- ตะกร้าสินค้าที่เก็บผ่าน session
- ตรวจสอบโค้ดส่วนลดและส่วนลดตามจำนวนอัตโนมัติ
- ชำระเงินพร้อมระบุที่อยู่จัดส่งและ PromptPay QR
- ประวัติการสั่งซื้อและจัดการโปรไฟล์

### พนักงาน (Staff)
- ระบบขายหน้าร้าน (POS Terminal)
- รับชำระเงินสดพร้อมคำนวณเงินทอนอัตโนมัติ
- จัดการสต๊อกสินค้าและปรับจำนวนคงเหลือ
- บันทึกความเคลื่อนไหวของสต๊อก (Stock Movement Log)
- ประวัติการขายพร้อมกรองตามวันที่/เดือน
- ส่งออกข้อมูลยอดขาย

### ผู้ดูแลระบบ (Admin)
- แดชบอร์ดแสดงยอดขาย จำนวนคำสั่งซื้อ จำนวนสมาชิก และจำนวนสินค้า
- กราฟยอดขาย 7 วันล่าสุด
- จัดการสินค้าแบบครบวงจร (CRUD)
- จัดการคำสั่งซื้อและอัปเดตสถานะ
- จัดการผู้ใช้งาน

---

## สถาปัตยกรรมระบบ

แอปพลิเคชันนี้ออกแบบตามรูปแบบ MVC:

| ส่วน | หน้าที่ |
|---|---|
| `Controllers/` | จัดการ request และ business logic — `HomeController`, `UserController`, `StaffController`, `AdminController` |
| `Models/` | Entity classes และ view models |
| `Models/Db/` | `Csi402dbContext` — Entity Framework Core database context |
| `Views/` | Razor views จัดกลุ่มตาม controller |
| `wwwroot/` | ไฟล์ static — CSS, JavaScript, รูปภาพ, ไลบรารี |

### ฐานข้อมูล

ตารางหลัก 6 ตารางที่จัดการผ่าน Entity Framework Core:

`Users` · `Products` · `Orders` · `OrderDetails` · `Promotions` · `StockLogs`

---

## เอกสารประกอบ (Documentation)

เอกสารวิเคราะห์ระบบฉบับเต็ม: [`docs/CakeShop_DFD_Flowchart.pdf`](docs/CakeShop_DFD_Flowchart.pdf)

- **Context Diagram (DFD Level 0)** — ผู้เกี่ยวข้องภายนอกทั้ง 3 กลุ่ม (ลูกค้า, พนักงาน, ผู้ดูแลระบบ) และทิศทางการไหลของข้อมูลกับระบบ
- **DFD Level 1** — 6 กระบวนการ (สมาชิก/ล็อกอิน, จัดการสินค้า, ประมวลผลคำสั่งซื้อออนไลน์, POS, จัดการคลังสินค้า, ออกรายงาน) เทียบกับ 6 แหล่งจัดเก็บข้อมูล
- **Flowchart** — สมัครสมาชิกและเข้าสู่ระบบ, การสั่งซื้อสินค้าออนไลน์, การขายหน้าร้าน (POS)

---

## วิธีติดตั้งและรันโปรเจกต์

### สิ่งที่ต้องมีก่อน
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [MySQL Server](https://dev.mysql.com/downloads/mysql/) (8.0+ / 9.x)

### ขั้นตอนติดตั้ง

1. Clone repository
   ```bash
   git clone https://github.com/Pstsrppt/cakeshop-ecommerce.git
   cd cakeshop-ecommerce
   ```

2. สร้างฐานข้อมูล MySQL ชื่อ `csi402db` และ import schema จากนั้นรันคำสั่งเพิ่มคอลัมน์:
   ```bash
   mysql -u root -p csi402db < migration_add_columns.sql
   ```

3. สร้างไฟล์ `appsettings.Development.json` ที่ root ของโปรเจกต์ (ไฟล์นี้อยู่ใน `.gitignore` แล้ว) พร้อมใส่ connection string ของตัวเอง:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "server=localhost;user=root;password=YOUR_PASSWORD;database=csi402db;AllowPublicKeyRetrieval=True"
     }
   }
   ```
   > ต้องใส่ `AllowPublicKeyRetrieval=True` เมื่อเชื่อมต่อ MySQL 8+/9 ที่ใช้ auth plugin เริ่มต้นเป็น `caching_sha2_password` โดยไม่ได้เปิด SSL

4. Restore และรันโปรเจกต์
   ```bash
   dotnet restore
   dotnet run
   ```

5. เปิดใช้งานได้ที่ `http://localhost:5000`

---

## หมายเหตุด้านความปลอดภัย

- รหัสผ่านฐานข้อมูลถูกแยกออกจากซอร์สโค้ด โดยเก็บไว้ใน `appsettings.Development.json` (ไม่ถูก commit) ดูรูปแบบที่ต้องตั้งค่าได้จาก `appsettings.json`
- ⚠️ รหัสผ่านผู้ใช้ปัจจุบันถูกเก็บและเปรียบเทียบแบบ plain text (ใน `UserController.Login` / `ChangePassword`) เหมาะสำหรับงานเรียน/เดโมเท่านั้น ห้ามนำไปใช้งานจริงหรือใช้กับข้อมูลผู้ใช้จริงโดยไม่เพิ่มระบบ hash รหัสผ่านก่อน (เช่น BCrypt หรือ ASP.NET Core Identity)

---

## ผู้พัฒนา

**พงศธร ศิริพรพิทักษ์ (Pongsathorn Siriprompitak)**
สาขาวิทยาการคอมพิวเตอร์ มหาวิทยาลัยศรีปทุม
GitHub: [@Pstsrppt](https://github.com/Pstsrppt)
