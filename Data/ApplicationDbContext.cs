using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartStock.Models;

namespace SmartStock.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<InventoryProductEntity> InventoryProducts => Set<InventoryProductEntity>();
    public DbSet<InventorySupplierEntity> InventorySuppliers => Set<InventorySupplierEntity>();
    public DbSet<InventoryTransactionEntity> InventoryTransactions => Set<InventoryTransactionEntity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<InventoryProductEntity>(entity =>
        {
            entity.HasKey(product => product.Id);
            entity.Property(product => product.Name).HasMaxLength(100).IsRequired();
            entity.Property(product => product.Sku).HasMaxLength(40).IsRequired();
            entity.HasIndex(product => product.Sku).IsUnique();
            entity.Property(product => product.Category).HasMaxLength(60).IsRequired();
            entity.Property(product => product.Supplier).HasMaxLength(100).IsRequired();
            entity.HasOne<InventorySupplierEntity>()
                .WithMany()
                .HasForeignKey(product => product.Supplier)
                .HasPrincipalKey(supplier => supplier.Name)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<InventorySupplierEntity>(entity =>
        {
            entity.HasKey(supplier => supplier.Name);
            entity.Property(supplier => supplier.Name).HasMaxLength(100).UseCollation("NOCASE");
        });

        builder.Entity<InventoryTransactionEntity>(entity =>
        {
            entity.HasKey(transaction => transaction.Id);
            entity.Property(transaction => transaction.Type).HasMaxLength(3).IsRequired();
            entity.Property(transaction => transaction.Reference).HasMaxLength(60).IsRequired();
            entity.HasOne<InventoryProductEntity>()
                .WithMany()
                .HasForeignKey(transaction => transaction.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(transaction => transaction.Date);
        });
    }

    public void EnsureInventoryTablesCreated()
    {
        Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS "InventorySuppliers" (
                "Name" TEXT NOT NULL COLLATE NOCASE CONSTRAINT "PK_InventorySuppliers" PRIMARY KEY
            );
            CREATE TABLE IF NOT EXISTS "InventoryProducts" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_InventoryProducts" PRIMARY KEY AUTOINCREMENT,
                "Name" TEXT NOT NULL,
                "Sku" TEXT NOT NULL COLLATE NOCASE,
                "Category" TEXT NOT NULL,
                "Supplier" TEXT NOT NULL COLLATE NOCASE,
                "OnHand" INTEGER NOT NULL,
                "ReorderLevel" INTEGER NOT NULL,
                "UnitCost" TEXT NOT NULL,
                CONSTRAINT "FK_InventoryProducts_InventorySuppliers_Supplier"
                    FOREIGN KEY ("Supplier") REFERENCES "InventorySuppliers" ("Name") ON DELETE RESTRICT
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_InventoryProducts_Sku" ON "InventoryProducts" ("Sku");
            CREATE TABLE IF NOT EXISTS "InventoryTransactions" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_InventoryTransactions" PRIMARY KEY AUTOINCREMENT,
                "Date" TEXT NOT NULL,
                "Type" TEXT NOT NULL,
                "ProductId" INTEGER NOT NULL,
                "Quantity" INTEGER NOT NULL,
                "Reference" TEXT NOT NULL,
                CONSTRAINT "FK_InventoryTransactions_InventoryProducts_ProductId"
                    FOREIGN KEY ("ProductId") REFERENCES "InventoryProducts" ("Id") ON DELETE RESTRICT
            );
            CREATE INDEX IF NOT EXISTS "IX_InventoryTransactions_Date" ON "InventoryTransactions" ("Date");
            """);
    }
}
