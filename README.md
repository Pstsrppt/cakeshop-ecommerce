<!-- Language: English | [ภาษาไทย](README.th.md) -->

# CakeShop — E-Commerce & Point-of-Sale Web Application

A full-stack e-commerce and point-of-sale system for a cake shop, built with ASP.NET Core MVC, C#, MySQL, and Entity Framework Core. The system serves three user roles — Customer, Staff, and Admin — covering the complete flow from browsing and ordering through in-store checkout and inventory management.

**Tech Stack:** ASP.NET Core MVC · C# · MySQL · Entity Framework Core · Razor Views · Bootstrap · Chart.js

---

## Features

### Customer
- Registration and login with role-based redirection
- Browse the product catalogue, with sold-out state handling
- Shopping cart stored in session
- Promo code validation and automatic quantity discounts
- Checkout with delivery address and PromptPay QR payment
- Order history and profile management

### Staff
- Point-of-sale terminal for in-store sales
- Cash payment with automatic change calculation
- Inventory management and stock adjustment
- Stock movement log
- Sales history with date and month filtering
- Export sales data

### Admin
- Dashboard with revenue, order count, member count, and product count
- 7-day sales chart
- Full CRUD for products
- Order management and status updates
- User management

---

## Architecture

The application follows the MVC pattern:

| Layer | Responsibility |
|---|---|
| `Controllers/` | Handle requests and business logic — `HomeController`, `UserController`, `StaffController`, `AdminController` |
| `Models/` | Entity classes and view models |
| `Models/Db/` | `Csi402dbContext` — the Entity Framework Core database context |
| `Views/` | Razor views, organised by controller |
| `wwwroot/` | Static assets — CSS, JavaScript, images, libraries |

### Database

Six main tables managed through Entity Framework Core:

`Users` · `Products` · `Orders` · `OrderDetails` · `Promotions` · `StockLogs`

---

## Documentation

System analysis documents are available in [`docs/`](docs/):

- **Context Diagram (DFD Level 0)** — system overview with external entities and data stores
- **DFD Level 1** — decomposed processes for Customer (9), Staff (6), and Admin (4)
- **Flowcharts** — registration and login, shopping and checkout, POS and inventory, admin operations

> _Diagrams pending upload — coming soon._

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [MySQL Server](https://dev.mysql.com/downloads/mysql/) (8.0+ / 9.x)

### Setup

1. Clone the repository
   ```bash
   git clone https://github.com/Pstsrppt/cakeshop-ecommerce.git
   cd cakeshop-ecommerce
   ```

2. Create a MySQL database named `csi402db` and import the schema, then apply the extra columns:
   ```bash
   mysql -u root -p csi402db < migration_add_columns.sql
   ```

3. Create `appsettings.Development.json` in the project root (git-ignored) with your own connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "server=localhost;user=root;password=YOUR_PASSWORD;database=csi402db;AllowPublicKeyRetrieval=True"
     }
   }
   ```
   > `AllowPublicKeyRetrieval=True` is required when connecting to MySQL 8+/9 using the default `caching_sha2_password` plugin without SSL.

4. Restore and run
   ```bash
   dotnet restore
   dotnet run
   ```

5. Open `http://localhost:5000` in a browser

---

## Security Notes

- Database credentials are kept out of source control via `appsettings.Development.json` (git-ignored) — see `appsettings.json` for the expected shape.
- ⚠️ User passwords are currently stored and compared as plain text (`UserController.Login` / `ChangePassword`). This is acceptable for coursework demo purposes only — do not deploy to production or reuse real user data without adding proper password hashing (e.g. BCrypt / ASP.NET Core Identity).

---

## Author

**Pongsathorn Siriprompitak**
Computer Science, Sripatum University
GitHub: [@Pstsrppt](https://github.com/Pstsrppt)
