-- ===================================================
-- Migration Script: เพิ่มคอลัมน์ ShippingAddress และ PaymentStatus ในตาราง orders
-- รันคำสั่งนี้ใน MySQL ก่อนเปิดเว็บ
-- ===================================================

ALTER TABLE `orders`
ADD COLUMN `ShippingAddress` VARCHAR(500) NULL AFTER `OrderStatus`,
ADD COLUMN `PaymentStatus` VARCHAR(20) NULL DEFAULT 'Unpaid' AFTER `ShippingAddress`;
