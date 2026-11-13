# SmartStock

SmartStock is an ASP.NET Core MVC inventory management app built with C#, Entity Framework Core, and SQL Server.

## Features

- Account registration, sign-in, and protected inventory pages
- Supplier, category, and product management with search and validation
- Stock IN and Stock OUT with transaction history
- Transaction filters for type, date range, product, SKU, supplier, and notes
- Live dashboard charts, inventory summaries, and low-stock alerts
- Filterable inventory and stock-movement reports with CSV export
- Database constraints and serialized stock updates to prevent negative inventory and lost updates

## Requirements

- .NET 10 SDK
- SQL Server LocalDB (included with Visual Studio), or a SQL Server instance

## Run locally

Open PowerShell in this directory and run:

```powershell
dotnet restore
dotnet tool restore
dotnet tool run dotnet-ef database update
dotnet run
```

Open the HTTP URL printed by `dotnet run` (the included launch profile uses `http://localhost:5188`). Register an account from the sign-in page to access the inventory.

The default connection string in `appsettings.json` uses SQL Server LocalDB and a database named `SmartStockDb`. For a different database, override `ConnectionStrings__DefaultConnection` with an environment variable. For example, in PowerShell:

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=YOUR_SERVER;Database=SmartStockDb;Trusted_Connection=True;TrustServerCertificate=True"
dotnet run
```

Never commit production credentials.

Account registration is open in this demo. Before deploying to a public server, decide whether registration should remain open and configure HTTPS and a production database connection.

## Typical workflow

1. Register an account and sign in.
2. Add a supplier.
3. Add categories and products. Products can optionally be assigned to a category and start at zero quantity.
4. Record opening inventory and deliveries with Stock IN.
5. Record issued inventory with Stock OUT.
6. Review the dashboard, transaction history, low-stock alerts, and reports. Reports can be exported to CSV.

Product quantities can only be changed through stock operations. Products or suppliers with on-hand stock or transaction history are protected from deletion.

## Migrations

The repository includes EF Core migrations. After changing an entity or database configuration, create and apply a migration:

```powershell
dotnet tool run dotnet-ef migrations add MigrationName
dotnet tool run dotnet-ef database update
```

The repository-local EF CLI version is pinned to 10.0.0 in `dotnet-tools.json`.

## Build

```powershell
dotnet build
```
