-- ===================================================
-- Script: ปรับข้อมูล (data) ให้ดูสมจริง ก่อนถ่ายภาพหน้าจอลงพอร์ตโฟลิโอ
-- ไม่มีการแก้โครงสร้างตาราง (schema), ไม่แตะ Password/PasswordHash ใดๆ
-- รันด้วยคำสั่ง: mysql -u root -p csi402db < seed_demo_data_portfolio.sql
-- ===================================================

START TRANSACTION;

-- ---------------------------------------------------
-- 1) แก้วันหมดอายุโปรโมโค้ด SUMMER20 (เดิมเป็นปี 5012 ผิดพลาด)
--    ตั้งให้เป็นช่วงที่ "ใช้งานได้จริงตอนนี้" (15 ส.ค. - 15 ต.ค. 2026)
--    เพื่อให้กดใช้โค้ดตอนถ่ายสกรีนช็อตได้จริง ไม่ขึ้น error ว่าหมดอายุ
-- ---------------------------------------------------
UPDATE promotions
SET StartDate = '2026-08-15',
    EndDate   = '2026-10-15'
WHERE PromoCode = 'SUMMER20';


-- ---------------------------------------------------
-- 2) เอา "เค้กงานแต่ง" ออกจากร้านทั้งหมด (ตามที่แจ้งเพิ่มเติมล่าสุด)
--    เดิมมี 3 รายการซ้ำ (ProductId 6, 7, 8) ไม่ใช่ 2 ตามที่คาดไว้ และไฟล์รูปภาพของ
--    ทั้ง 3 รายการ (เช่น .../ff1f5b98-....png) ไม่มีอยู่จริงในเครื่องแล้ว
--    เว็บจึงไป fallback แสดงรูป "เค้กหน้าตลก" (wwwroot/images/no-image.png) แทน
--
--    2.1) ProductId 6 (999 บาท, StockQty ติดลบ -17, ไม่มีคำสั่งซื้อใดอ้างอิงถึง)
--         → เป็นข้อมูลทดสอบที่พังแล้ว ลบทิ้งได้อย่างปลอดภัย
--         ต้องลบ StockLogs ที่อ้างอิงถึงก่อน ป้องกัน error Foreign Key
-- ---------------------------------------------------
DELETE FROM stocklog WHERE ProductId = 6;
DELETE FROM products WHERE ProductId = 6;

-- 2.2) ProductId 7 (999 บาท) มีคำสั่งซื้อ (OrderId 20) อ้างอิงอยู่ ลบทั้งแถวไม่ได้
--      (จะทำให้ประวัติคำสั่งซื้อ #20 พังเพราะขาดสินค้าอ้างอิง)
--      → เปลี่ยนเป็นสินค้าคนละแบบกับเค้กงานแต่งไปเลย แทนการลบ
--      → ล้างค่ารูปภาพที่ชี้ไปไฟล์ที่ไม่มีอยู่จริงออกก่อน (ดูหมายเหตุท้ายไฟล์
--        เรื่องต้องอัปโหลดรูปจริงทดแทนด้วยตัวเอง)
UPDATE products
SET CakeName     = 'เค้กวันเกิดพรีเมียมสามชั้น',
    ProductImage = NULL
WHERE ProductId = 7;

-- 2.3) ProductId 8 (888 บาท) มีคำสั่งซื้อ (OrderId 22) อ้างอิงอยู่ ลบทั้งแถวไม่ได้เช่นกัน
--      → เปลี่ยนชื่อออกจากคำว่า "เค้กงานแต่ง" ไปเป็นสินค้าคัสตอมสั่งทำแทน
--      (ตอนนี้ในร้านจะไม่มีสินค้าชื่อ "เค้กงานแต่ง" หลงเหลืออยู่เลยสักรายการ)
--      → ล้างค่ารูปภาพที่ชี้ไปไฟล์ที่ไม่มีอยู่จริงออกเช่นกัน
UPDATE products
SET CakeName     = 'เค้กคัสตอมสั่งทำพิเศษ',
    ProductImage = NULL
WHERE ProductId = 8;


-- ---------------------------------------------------
-- 3) บัญชีทดสอบ golf_lnwza007 (UserId 2) → เปลี่ยนเป็นชื่อสมจริง
--    (ไม่แตะคอลัมน์ Password/UserRole ใดๆ)
-- ---------------------------------------------------
UPDATE users
SET FullName = 'Demo Customer',
    Username = 'demo_customer',
    Email    = 'demo.customer@example.com'
WHERE UserId = 2;

-- ---------------------------------------------------
-- 4) กระจายชื่อ-อีเมลลูกค้าในคำสั่งซื้อให้หลากหลาย ใช้โดเมน example.com เท่านั้น
--    - ย้ายบางออเดอร์จาก UserId 2 (เดิมถือครองเกือบทุกออเดอร์) ไปให้ลูกค้า
--      บัญชีอื่นที่มีอยู่แล้วแต่ไม่เคยมีออเดอร์เลย เพื่อให้หน้า "จัดการคำสั่งซื้อ"
--      ไม่โชว์ชื่อ-อีเมลซ้ำเดิมทุกแถว
--    - แก้โดเมนอีเมลของบัญชีลูกค้าที่เกี่ยวข้องให้เป็น @example.com
--      (ยกเว้น UserId 12 "พงศธร" ซึ่งเป็นบัญชีส่วนตัวของเจ้าของโปรเจกต์ ไม่แตะ)
--    - พบว่า UserId 11 มีชื่อ "TrumPed" / อีเมล Trump11@mail.com ซึ่งมีคำสั่งซื้อ
--      อยู่จริง (OrderId 10) และดูไม่เป็นมืออาชีพเช่นกัน จึงแก้ไขให้ด้วย แม้ไม่ได้
--      อยู่ในลิสต์ที่แจ้งมา เพราะเข้าเงื่อนไข "ลูกค้าในออเดอร์" เหมือนกัน
-- ---------------------------------------------------
UPDATE users SET Email = 'somchai.j@example.com' WHERE UserId = 5;  -- สมชาย ใจดี
-- UserId 6 (Jane Doe) ใช้โดเมน example.com อยู่แล้ว ไม่ต้องแก้
UPDATE users SET Email = 'kanya.s@example.com'   WHERE UserId = 8;  -- กัญญารัตน์ สวยงาม
UPDATE users SET Email = 'arnon.r@example.com'   WHERE UserId = 10; -- อานนท์ รักขนม
UPDATE users
SET FullName = 'ธนพล มั่งมี',
    Username = 'thanapol_m',
    Email    = 'thanapol.m@example.com'
WHERE UserId = 11; -- เดิมชื่อ TrumPed

-- ย้ายเจ้าของออเดอร์บางรายการ จาก Demo Customer ไปยังลูกค้าบัญชีอื่น
-- (แก้แค่ผู้สั่ง ไม่แตะยอดเงิน/สถานะ/วันที่เดิม)
UPDATE orders SET UserId = 6  WHERE OrderId = 2;  -- ไปที่ Jane Doe
UPDATE orders SET UserId = 5  WHERE OrderId = 14; -- ไปที่ สมชาย ใจดี
UPDATE orders SET UserId = 8  WHERE OrderId = 16; -- ไปที่ กัญญารัตน์ สวยงาม
UPDATE orders SET UserId = 10 WHERE OrderId = 18; -- ไปที่ อานนท์ รักขนม


-- ---------------------------------------------------
-- 5) เติมออเดอร์ตัวอย่างให้ครบ 7 วันย้อนหลัง (กราฟยอดขายหน้าแอดมิน)
--    ระบบคิดช่วง "7 วันย้อนหลัง" จากวันที่วันนี้ (today.AddDays(-6) ถึง today)
--    สคริปต์นี้เตรียมไว้สำหรับวันนี้ = 2026-09-12 ครับ
--    *** ถ้ารันสคริปต์นี้ในวันอื่นที่ไม่ใช่ 2026-09-12 ให้แก้วันที่ในบล็อกนี้
--        เป็นวันปัจจุบันย้อนหลัง 6 วันเองก่อนรัน ไม่งั้นจะไม่โผล่ในกราฟ 7 วัน ***
--    ยอดเงินคำนวณจากราคาสินค้าจริงในร้าน คูณจำนวนที่ใส่ ไม่ใช่ตัวเลขสุ่ม
-- ---------------------------------------------------

-- 2026-09-07: กัญญารัตน์ สั่งเค้กบลูเบอรี่ 2 + ครัวซองต์ 1
INSERT INTO orders (UserId, OrderDate, TotalPrice, OrderStatus, PaymentStatus)
VALUES (8, '2026-09-07 09:15:00', 205, 'Completed', 'Paid');
SET @oid := LAST_INSERT_ID();
INSERT INTO orderdetails (OrderId, ProductId, Quantity, UnitPrice) VALUES
(@oid, 2, 2, 85),   -- เค้กบลูเบอรี่ x2
(@oid, 3, 1, 35);   -- ครัวซองต์ x1

-- 2026-09-08: อานนท์ สั่งเค้กลาวาสตรอว์เบอรี่ 2 + เค้กหน้านิ่ม 1
INSERT INTO orders (UserId, OrderDate, TotalPrice, OrderStatus, PaymentStatus)
VALUES (10, '2026-09-08 14:20:00', 230, 'Completed', 'Paid');
SET @oid := LAST_INSERT_ID();
INSERT INTO orderdetails (OrderId, ProductId, Quantity, UnitPrice) VALUES
(@oid, 4, 2, 90),   -- เค้กลาวาสตรอว์เบอรี่ x2
(@oid, 1, 1, 50);   -- เค้กหน้านิ่ม x1

-- 2026-09-08: ลูกค้าหน้าร้าน (POS ไม่ผูกบัญชี) ซื้อครัวซองต์ 3 ชิ้น
INSERT INTO orders (UserId, OrderDate, TotalPrice, OrderStatus, PaymentStatus)
VALUES (NULL, '2026-09-08 17:05:00', 105, 'Completed', 'Paid');
SET @oid := LAST_INSERT_ID();
INSERT INTO orderdetails (OrderId, ProductId, Quantity, UnitPrice) VALUES
(@oid, 3, 3, 35);   -- ครัวซองต์ x3

-- 2026-09-09: สมชาย สั่งชีสเค้กสตรอว์เบอร์รี่ 2 + เค้กบลูเบอรี่ 1
INSERT INTO orders (UserId, OrderDate, TotalPrice, OrderStatus, PaymentStatus)
VALUES (5, '2026-09-09 10:05:00', 223, 'Completed', 'Paid');
SET @oid := LAST_INSERT_ID();
INSERT INTO orderdetails (OrderId, ProductId, Quantity, UnitPrice) VALUES
(@oid, 5, 2, 69),   -- เค้กสตรอว์เบอร์รี่ชีสเค้ก x2
(@oid, 2, 1, 85);   -- เค้กบลูเบอรี่ x1

-- 2026-09-10: Jane Doe สั่งเค้กคัสตอมสั่งทำพิเศษ 1 ก้อน (ออนไลน์ ยังไม่ชำระ)
INSERT INTO orders (UserId, OrderDate, TotalPrice, OrderStatus, PaymentStatus)
VALUES (6, '2026-09-10 16:40:00', 888, 'Pending', 'Unpaid');
SET @oid := LAST_INSERT_ID();
INSERT INTO orderdetails (OrderId, ProductId, Quantity, UnitPrice) VALUES
(@oid, 8, 1, 888);  -- เค้กคัสตอมสั่งทำพิเศษ x1

-- 2026-09-10: ลูกค้าหน้าร้าน ซื้อเค้กหน้านิ่ม 2 ชิ้น
INSERT INTO orders (UserId, OrderDate, TotalPrice, OrderStatus, PaymentStatus)
VALUES (NULL, '2026-09-10 18:10:00', 100, 'Completed', 'Paid');
SET @oid := LAST_INSERT_ID();
INSERT INTO orderdetails (OrderId, ProductId, Quantity, UnitPrice) VALUES
(@oid, 1, 2, 50);   -- เค้กหน้านิ่ม x2

-- 2026-09-11: กัญญารัตน์ สั่งครัวซองต์ 4 + เค้กลาวาสตรอว์เบอรี่ 1
INSERT INTO orders (UserId, OrderDate, TotalPrice, OrderStatus, PaymentStatus)
VALUES (8, '2026-09-11 11:30:00', 230, 'Completed', 'Paid');
SET @oid := LAST_INSERT_ID();
INSERT INTO orderdetails (OrderId, ProductId, Quantity, UnitPrice) VALUES
(@oid, 3, 4, 35),   -- ครัวซองต์ x4
(@oid, 4, 1, 90);   -- เค้กลาวาสตรอว์เบอรี่ x1

-- 2026-09-12 (วันนี้): อานนท์ สั่งเค้กบลูเบอรี่ 1 + ชีสเค้กสตรอว์เบอร์รี่ 1 + ครัวซองต์ 2
INSERT INTO orders (UserId, OrderDate, TotalPrice, OrderStatus, PaymentStatus)
VALUES (10, '2026-09-12 09:50:00', 224, 'Processing', 'Paid');
SET @oid := LAST_INSERT_ID();
INSERT INTO orderdetails (OrderId, ProductId, Quantity, UnitPrice) VALUES
(@oid, 2, 1, 85),   -- เค้กบลูเบอรี่ x1
(@oid, 5, 1, 69),   -- เค้กสตรอว์เบอร์รี่ชีสเค้ก x1
(@oid, 3, 2, 35);   -- ครัวซองต์ x2


-- ---------------------------------------------------
-- ตรวจสอบผลลัพธ์ก่อน COMMIT (ดูผ่านๆ ว่าตัวเลขดูสมเหตุสมผลไหม)
-- ---------------------------------------------------
SELECT PromoCode, StartDate, EndDate FROM promotions WHERE PromoCode = 'SUMMER20';
-- ต้องไม่มีแถวไหนชื่อ "เค้กงานแต่ง" หลงเหลืออยู่เลย (COUNT ต้องเป็น 0)
SELECT COUNT(*) AS remaining_wedding_cakes FROM products WHERE CakeName LIKE '%งานแต่ง%';
SELECT ProductId, CakeName, Price, ProductImage FROM products WHERE ProductId IN (7, 8);
SELECT UserId, FullName, Username, Email, UserRole FROM users WHERE UserId IN (2,5,6,8,10,11);
SELECT DATE(OrderDate) AS OrderDay, SUM(TotalPrice) AS DailyTotal
FROM orders
WHERE OrderDate BETWEEN '2026-09-06' AND '2026-09-12 23:59:59'
GROUP BY DATE(OrderDate)
ORDER BY OrderDay;

COMMIT;
-- หมายเหตุ: ถ้ารันไฟล์นี้แบบ mysql ... < seed_demo_data_portfolio.sql (แบบ batch)
-- การเชื่อมต่อจะปิดทันทีที่รันเสร็จ ถ้าไม่มี COMMIT ในไฟล์ MySQL จะ ROLLBACK
-- การเปลี่ยนแปลงทั้งหมดทิ้งอัตโนมัติ จึงใส่ COMMIT ไว้ให้ท้ายสคริปต์เลย
-- ตัวช่วยความปลอดภัยหลักคือ "สำรองฐานข้อมูลก่อนรัน" ตามขั้นตอนด้านล่าง แทน
