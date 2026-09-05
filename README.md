# 🎂 CakeShop — Online Bakery Management System

A full-stack **e-commerce and inventory management platform** for a cake shop, built with **ASP.NET Core MVC** and **MySQL**. The system supports three distinct roles — Customer, Staff, and Admin — each with a dedicated workflow, from browsing and ordering cakes to managing stock, promotions, and sales reports.

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)
![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?logo=dotnet&logoColor=white)
![Entity Framework Core](https://img.shields.io/badge/EF%20Core-9.0-512BD4)
![MySQL](https://img.shields.io/badge/MySQL-9.x-4479A1?logo=mysql&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5-7952B3?logo=bootstrap&logoColor=white)

---

## 📖 Overview

CakeShop is a role-based web application that digitizes the day-to-day operations of a bakery business — customer ordering, staff order fulfillment, and admin oversight — all in one platform.

| Role | Can do |
|------|--------|
| 🛍️ **Customer** | Browse the catalog, manage a shopping cart, apply promo codes, checkout, track order history, and manage their profile |
| 👷 **Staff** | Fulfill orders, manage inventory & stock levels, verify payments, run promotions, and view a sales calendar / stock reports |
| 🛠️ **Admin** | Full product CRUD, order and user management, role assignment, and a sales dashboard |

---

## ✨ Features

### Customer
- Product catalog with product detail pages
- Shopping cart (add / increase / decrease / remove items)
- Promo code application at checkout
- Checkout with shipping address
- Order history tracking
- Profile management & password change

### Staff
- Inventory management with stock quantity updates
- Order fulfillment workflow (status updates, payment verification)
- Sales calendar and stock reports
- Promotion management (create / delete promo codes)
- Order export

### Admin
- Product management (create, edit, delete, image upload)
- Order management with date filtering
- User management with role assignment
- Sales dashboard (last 7 days sales chart)

### Platform
- Session-based authentication with role-based access control
- Image upload for product listings
- Responsive UI built with Bootstrap 5

---

## 🏗️ Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC (.NET 10) |
| ORM | Entity Framework Core 9 |
| Database | MySQL (via [Pomelo.EntityFrameworkCore.MySql](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql)) |
| Frontend | Razor Views, Bootstrap 5, jQuery, jQuery Validation |
| Auth | Session-based (`ASP.NET Core Session`) |

---

## 📂 Project Structure

```
Project_CakeShop_66095681/
├── Controllers/
│   ├── HomeController.cs        # Landing page
│   ├── UserController.cs        # Customer: auth, catalog, cart, checkout
│   ├── StaffController.cs       # Staff: inventory, orders, promotions
│   └── AdminController.cs       # Admin: products, orders, users, dashboard
├── Models/
│   ├── Db/                      # EF Core entities (Product, Order, User, ...)
│   └── ...                      # View models (Login, Register, CartItem, ...)
├── Views/
│   ├── User/ Staff/ Admin/      # Role-specific views
│   └── Shared/                  # Shared layout & navbars per role
├── wwwroot/                     # Static assets (css, js, images, client libs)
├── migration_add_columns.sql    # Manual SQL migration for orders table
└── Program.cs                   # App configuration & DI setup
```

---

## 🗄️ Database Schema

Core entities managed via EF Core (`Csi402dbContext`):

- **users** — account info, credentials, role (`Customer` / `Staff` / `Admin`)
- **products** — cake catalog: name, price, discount, stock quantity, expiry date, image
- **orders** — customer orders: status, payment status, shipping address, total price
- **orderdetails** — line items linking orders to products
- **promotions** — promo codes with discount percentage
- **stocklog** — inventory movement audit log

---

## 🚀 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [MySQL Server](https://dev.mysql.com/downloads/mysql/) (8.0+ / 9.x)

### 1. Clone the repository
```bash
git clone https://github.com/Pstsrppt/cakeshop-ecommerce.git
cd cakeshop-ecommerce
```

### 2. Create the database
Create a MySQL database named `csi402db`, then apply the schema (via the EF Core model or your own SQL dump), followed by:
```bash
mysql -u root -p csi402db < migration_add_columns.sql
```

### 3. Configure your connection string
Connection strings are **not** committed to source control. Create `appsettings.Development.json` in the project root (this file is git-ignored):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;user=root;password=YOUR_PASSWORD;database=csi402db;AllowPublicKeyRetrieval=True"
  }
}
```

> **Note:** `AllowPublicKeyRetrieval=True` is required when connecting to MySQL 8+/9 servers using the default `caching_sha2_password` auth plugin without SSL.

### 4. Restore & run
```bash
dotnet restore
dotnet run
```

The app will be available at `http://localhost:5000`.

---

## 🔒 Security Notes

- Database credentials are kept out of source control via `appsettings.Development.json` (git-ignored) — see `appsettings.json` for the expected configuration shape.
- ⚠️ **User passwords are currently stored and compared as plain text** (`UserController.Login` / `ChangePassword`). This is acceptable for coursework demo purposes only — **do not deploy this to production or reuse real user data without adding proper password hashing (e.g. BCrypt/ASP.NET Core Identity)**.

---

## 📜 License

This project was built for educational purposes (CSI402 coursework).
