using AbuAmenPharma.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AbuAmenPharma.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Unit> Units => Set<Unit>();
        public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
        public DbSet<Item> Items => Set<Item>();
        public DbSet<ItemBatch> ItemBatches => Set<ItemBatch>();
        public DbSet<StockMovement> StockMovements => Set<StockMovement>();

        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<Purchase> Purchases => Set<Purchase>();
        public DbSet<PurchaseLine> PurchaseLines => Set<PurchaseLine>();

        public DbSet<PurchaseReturn> PurchaseReturns => Set<PurchaseReturn>();
        public DbSet<PurchaseReturnLine> PurchaseReturnLines => Set<PurchaseReturnLine>();

        public DbSet<Salesman> Salesmen => Set<Salesman>();
        public DbSet<Customer> Customers => Set<Customer>();

        public DbSet<Sale> Sales => Set<Sale>();
        public DbSet<SaleLine> SaleLines => Set<SaleLine>();
        public DbSet<SaleAllocation> SaleAllocations => Set<SaleAllocation>();

        public DbSet<SaleReturn> SaleReturns => Set<SaleReturn>();
        public DbSet<SaleReturnLine> SaleReturnLines => Set<SaleReturnLine>();
        public DbSet<SaleReturnAllocation> SaleReturnAllocations => Set<SaleReturnAllocation>();

        public DbSet<CustomerLedger> CustomerLedgers => Set<CustomerLedger>();

        public DbSet<CustomerReceipt> CustomerReceipts => Set<CustomerReceipt>();
        public DbSet<CustomerReceiptAllocation> CustomerReceiptAllocations => Set<CustomerReceiptAllocation>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Item>()
                .HasIndex(x => x.BarCode)
                .IsUnique(false);

            builder.Entity<ItemBatch>()
                .HasIndex(x => new { x.ItemId, x.BatchNo })
                .IsUnique();

            builder.Entity<StockMovement>()
                .HasIndex(x => new { x.ItemId, x.BatchId, x.Date });
        }
    }
}
