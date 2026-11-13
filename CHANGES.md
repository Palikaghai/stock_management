# SmartStock branch changes

This folder contains 28 copied files from the SmartStock integration. It is not a full copy of the project.

## Apply these files

Copy the folders and files in this directory into the root of the SmartStock project on your branch, keeping the directory structure. Review the resulting changes before committing.

If these older files exist on the branch, remove them because the dashboard now uses `DashboardViewModel` and the Home route redirects to Dashboard:

- `Models/DashboardStats.cs`
- `Views/Home/Index.cshtml`

## Included changes

- Database-backed dashboard charts and summary cards.
- Category management and optional category assignment on products.
- Inventory and stock-movement reports with CSV export.
- Dashboard-style layout, styles, and navigation updates.
- A database migration for categories and the optional product category relationship.
- README updates for the added functionality.

## Verify on the branch

From the SmartStock project directory, run:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef database update
dotnet build
```

The migration adds a nullable `Products.CategoryId`, so existing products can remain uncategorized.
