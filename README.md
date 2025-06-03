# BudgetPilot

**Diploma Project**  
**Student:** Ryzleitsev Myron  
**Group:** 545a  
**University:** National Aerospace University "Kharkiv Aviation Institute" (NAU "KhAI")  
**Department:** Department of Computer Systems, Networks and Cybersecurity (503)  
**Specialty:** Computer Engineering (123)  

**GitHub Repository:**  
https://github.com/mryzleitsev/BudgetPilot  

**Live Site / Personal Website:**  
https://mryzleitsev.space/  

---

## Table of Contents

1. [Project Overview](#project-overview)  
2. [Key Features](#key-features)  
3. [Technology Stack](#technology-stack)  
4. [Development Environment](#development-environment)  
5. [Installation & Setup](#installation--setup)  
6. [Project Structure](#project-structure)  
7. [Usage Examples](#usage-examples)  
8. [Future Enhancements](#future-enhancements)  
9. [Author](#author)  

---

## Project Overview

**BudgetPilot** is a diploma project designed to help users track and manage personal finances in a secure and user-friendly way. The web application is built on ASP.NET Core 8.0 (Razor Pages) with Entity Framework Core 8.0 (Code-First). Users can create multiple "wallet" accounts in different currencies, record one-time and recurring transactions, and view basic financial statistics with interactive charts.

This project was developed as part of the diploma requirements for the Department of Computer Systems, Networks and Cybersecurity (503) at NAU "KhAI" by student Ryzleitsev Myron (Group 545a).

---

## Key Features

1. **Authentication & Identity**  
   - Email/password registration and login (ASP.NET Core Identity).  
   - Custom-styled Login & Register pages scaffolded from Identity UI.  

2. **Wallet Management (Accounts)**  
   - **Create/Edit/Delete** "Accounts" (wallets) with fields:  
     - Name (e.g., "Cash", "Visa ****1234")  
     - Balance (decimal, precision 18,2)  
     - Currency (ISO code, e.g., USD, EUR)  
     - Type (Cash, Bank, E-wallet, etc.)  
     - Optional Description  
   - Account balance is automatically updated when transactions are added, edited, or removed.  

3. **Transaction Management**  
   - **Create/Edit/Delete** one-time transactions with fields:  
     - Date (DateTime)  
     - Amount (decimal)  
     - IsIncome (boolean checkbox → positive or negative adjustment)  
     - Category (pre-seeded: Food, Transport, Entertainment, etc.)  
     - Description (string)  
     - Linked Account (foreign key)  
   - Editing or deleting a transaction automatically rolls back the previous balance change and applies the updated amount.  

4. **Recurring Transactions**  
   - **Create/Edit/Delete** recurring transactions with fields:  
     - NextRunDate (DateTime)  
     - Amount (decimal, positive or negative)  
     - Frequency (enum: Daily, Weekly, Monthly)  
     - Category  
     - Description  
     - Linked Account  
     - IsActive (boolean)  
   - Recurring transactions are stored separately; as-is they do not immediately affect account balances.  

5. **Statistics Dashboard**  
   - **Current Balances per Account:** table view of each account's current balance and currency.  
   - **Monthly Transactions (Income/Expense):** interactive bar chart (Chart.js) showing last 12 months of income vs. expense per wallet.  
   - **This Month's Transaction Volume per Account:** doughnut chart visualizing sum of all transactions for the current month, by account.  
   - **Sum of Balances by Currency:** doughnut chart showing total balances aggregated by currency across all wallets.  
   - **Recurring Transactions Overview:** table summarizing each wallet's recurring transactions:  
     - This Month Total (sum of NextRunDate within the current month)  
     - Next Month Total (sum of NextRunDate within next calendar month)  
     - Total Planned (sum of all future occurrences)  

---

## Technology Stack

- **Framework & Language**  
  - ASP.NET Core 8.0 (Razor Pages)  
  - C# 12  

- **ORM & Database**  
  - Entity Framework Core 8.0 (Code-First, SQL Server)  
  - SQL Server LocalDB (or any SQL Server instance)  

- **Authentication / Identity**  
  - ASP.NET Core Identity (Identity UI)  
  - Scaffolded login/register pages using `dotnet aspnet-codegenerator identity`  

- **Front-End**  
  - HTML5, CSS3, Bootstrap 5 (Sass)  
  - Razor syntax for server-side rendering  
  - Chart.js (via CDN) for data visualization  

- **Development Tools & Libraries**  
  - JetBrains Rider (development environment)  
  - Microsoft.EntityFrameworkCore.Design (NuGet)  
  - Microsoft.EntityFrameworkCore.Tools (NuGet)  
  - Microsoft.AspNetCore.Identity.EntityFrameworkCore (NuGet)  
  - Microsoft.AspNetCore.Identity.UI (NuGet)  
  - Microsoft.VisualStudio.Web.CodeGeneration.Design (NuGet)  
  - dotnet-aspnet-codegenerator (global tool)  

---

## Development Environment

- **IDE:** JetBrains Rider  
- **.NET SDK:** 8.0.100 or newer  
- **NuGet Packages:**  
  - `Microsoft.EntityFrameworkCore.Design` (version 8.0.16)  
  - `Microsoft.EntityFrameworkCore.Tools` (version 8.0.16)  
  - `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (version 8.0.16)  
  - `Microsoft.AspNetCore.Identity.UI` (version 8.0.16)  
  - `Microsoft.VisualStudio.Web.CodeGeneration.Design` (version 8.0.16)  

> **Tip:** Use the `dotnet-aspnet-codegenerator` global tool to scaffold Identity pages (Login/Register) into your project folder under `Areas/Identity/Pages/Account`.

---

## Installation & Setup

Follow these steps to get BudgetPilot running on your local machine:

1. **Clone the Repository**  
   ```bash
   git clone https://github.com/mryzleitsev/BudgetPilot.git
   cd BudgetPilot
   ```

2. **Configure Connection String**
   * Open `appsettings.json`.
   * Edit `"ConnectionStrings:DefaultConnection"` to point to your SQL Server instance.

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BudgetPilotDB;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```

3. **Install EF Core Tools & Apply Migrations**
   * (If EF CLI is not installed)
     ```bash
     dotnet tool install --global dotnet-ef
     ```
   * In project root:
     ```bash
     dotnet ef database update
     ```
   * This will create the database and apply all pending migrations.

4. **Build & Run**
   ```bash
   dotnet build
   dotnet run
   ```
   * By default, the application will be accessible at `https://localhost:5001` (HTTPS) or `http://localhost:5000` (HTTP).

5. **Scaffold Identity Pages (if needed)**
   * If Identity pages (Login, Register) are not already present under `Areas/Identity`, you can generate them via:
     ```bash
     dotnet aspnet-codegenerator identity --dbContext ApplicationDbContext --files "Account.Login;Account.Register;Account.Logout"
     ```
   * Ensure that `_ViewStart.cshtml` in `Areas/Identity/Pages` references the main layout:
     ```csharp
     @{
         Layout = "/Pages/Shared/_Layout.cshtml";
     }
     ```

6. **Register & Login**
   * Navigate to `/Identity/Account/Register` to create a new user.
   * Then navigate to `/Identity/Account/Login` to sign in.

---

## Project Structure

```text
BudgetPilot/
├── Areas/
│   └── Identity/                            # ASP.NET Identity UI (Login, Register, etc.)
│       ├── Pages/
│       │   ├── Account/
│       │   │   ├── Login.cshtml
│       │   │   ├── Register.cshtml
│       │   │   ├── Logout.cshtml
│       │   │   └── … (other Identity pages)
│       │   └── Shared/
│       │       └── _Layout.cshtml            # Layout for Identity-specific pages
│       └── Data/
│           └── IdentityHostingStartup.cs
│
├── Data/
│   ├── ApplicationDbContext.cs              # EF Core DbContext + entity configurations
│   └── Migrations/                          # EF Core migrations folder
│
├── Models/
│   ├── Account.cs                           # Wallet entity
│   ├── Transaction.cs                       # One-time transaction entity
│   ├── RecurringTransaction.cs              # Recurring transaction entity
│   └── Category.cs                          # Category entity
│
├── Pages/
│   ├── _ViewImports.cshtml
│   ├── _ViewStart.cshtml
│   ├── Shared/
│   │   ├── _Layout.cshtml                   # Main application layout
│   │   └── _LoginPartial.cshtml             # Navbar login/logout links
│   │
│   ├── Index.cshtml                         # Home / Landing page
│   ├── Privacy.cshtml
│   │
│   ├── Accounts/
│   │   ├── Index.cshtml                     # Wallets list
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   └── Delete.cshtml
│   │
│   ├── Transactions/
│   │   ├── Index.cshtml                     # Transactions list
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   └── Delete.cshtml
│   │
│   ├── RecurringTransactions/
│   │   ├── Index.cshtml                     # Recurring transactions list
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   └── Delete.cshtml
│   │
│   ├── Statistics/
│   │   ├── Index.cshtml                     # Statistics dashboard
│   │   └── Index.cshtml.cs
│
├── wwwroot/
│   ├── css/
│   │   └── site.css                         # Site-wide styles (Bootstrap 5 + custom overrides)
│   ├── lib/
│   │   └── bootstrap/                       # Bootstrap (via LibMan or NPM)
│   └── js/
│       └── site.js                          # Custom JavaScript (if needed)
│
├── appsettings.json                         # App configuration and ConnectionStrings
├── Program.cs                               # ASP.NET Core startup / middleware pipeline
└── BudgetPilot.csproj                       # Project file
```

---

## Usage Examples

1. **Register & Login**
   * Navigate to `/Identity/Account/Register`, fill in email and password, then submit.
   * After successful registration, navigate to `/Identity/Account/Login` to sign in.

2. **Create a Wallet (Account)**
   * Go to **Wallets → + New Wallet**.
   * Fill in:
     * Name: "Cash"
     * Balance: 1000.00
     * Currency: "USD"
     * Type: "Cash"
     * Description: "Main wallet"
   * Click **Create**.

3. **Add a One-Time Transaction (Expense)**
   * Go to **Transactions → + New Transaction**.
   * Fill in:
     * Date: 2025-06-05
     * Amount: 250.00
     * Leave **IsIncome** unchecked (→ Expense)
     * Category: "Food"
     * Description: "Groceries"
     * Account: "Cash"
   * Click **Create**.
   * The "Cash" wallet balance will immediately decrease by 250.00.

4. **Add a Recurring Transaction (Monthly Subscription)**
   * Go to **Recurring Transactions → + New Recurring**.
   * Fill in:
     * Next Run Date: 2025-06-10
     * Amount: 50.00
     * Leave **IsIncome** unchecked (→ Expense)
     * Frequency: "Monthly"
     * Category: "Utilities"
     * Description: "Internet Subscription"
     * Account: "Cash"
     * IsActive: checked
   * Click **Create**.
   * This does not immediately alter any balances but will appear in the Recurring Overview.

5. **View Statistics**
   * Navigate to **Statistics**:
     * **Current Balances per Account** table will show each wallet's name, balance, and currency.
     * **Monthly Transactions** bar chart displays income vs. expense over the last 12 months per wallet.
     * **This Month's Transaction Volume** doughnut chart shows total transaction amounts for current month per wallet.
     * **Sum of Balances by Currency** doughnut chart shows aggregated balances by currency.
     * **Recurring Transactions Overview** table summarizes recurring transactions grouped by wallet (This Month, Next Month, All Future).

---

## Future Enhancements

* **Automated Recurring Transaction Processor**
  * A background service (hosted worker) that runs daily/weekly and auto-creates one-time `Transaction` records when `NextRunDate` is due, then updates `LastRunDate` and calculates the next `NextRunDate` based on `Frequency`.

* **Export & Reporting**
  * PDF/CSV export of transaction history and charts.
  * Email reporting (send a weekly or monthly summary to the user).

* **Advanced Filtering & Search**
  * Date-range filters, category filters, and search by description.
  * Custom date-range charts and multi-year trends.

* **User Roles & Administration**
  * Role-based authorization (e.g., "Admin" can manage all users).
  * Admin dashboard to view global statistics or user management.

* **Multi-Currency Conversion**
  * Real-time exchange rates (via a public API) to convert multi-currency wallets into a single base currency for consolidated reporting.

* **Bank API Integrations**
  * Sync transactions automatically from user's bank accounts (Plaid, OpenBanking, etc.).

---

## Author

```
Ryzleitsev Myron  
Group 545a  
Department of Computer Systems, Networks and Cybersecurity (503)  
Specialty: Computer Engineering (123)  
National Aerospace University "Kharkiv Aviation Institute" (NAU "KhAI")  
```

* **Email:** [miron04y@gmail.com](mailto:miron04y@gmail.com)
* **Personal Website:** [https://mryzleitsev.space/](https://mryzleitsev.space/)

---
