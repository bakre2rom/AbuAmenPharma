using AbuAmenPharma.Data;
using AbuAmenPharma.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace AbuAmenPharma.Services
{
    public class StockService : IStockService
    {
        private readonly ApplicationDbContext _context;

        public StockService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UpdateStockAsync(long batchId, decimal quantity, StockMovementType type, string refNo, string notes = null)
        {
            // Note: In our current model, StockMovement uses BatchId (int or long) and ItemId.
            // We need to fetch the batch to get the ItemId and current info.
            var batch = await _context.ItemBatches.FindAsync((int)batchId);
            if (batch == null) return false;

            var movement = new StockMovement
            {
                Date = DateTime.Now,
                ItemId = batch.ItemId,
                BatchId = batch.Id,
                RefId = long.TryParse(refNo, out long rid) ? rid : 0,
                Notes = notes,
                UnitCost = batch.PurchasePrice
            };

            // Map StockMovementType to StockRefType
            if (quantity > 0)
            {
                movement.QtyIn = quantity;
                movement.QtyOut = 0;
            }
            else
            {
                movement.QtyIn = 0;
                movement.QtyOut = Math.Abs(quantity);
            }

            // Set RefType based on business logic if needed, 
            // or we can extend the method to accept StockRefType directly.
            
            _context.StockMovements.Add(movement);
            return true;
        }

        public async Task<decimal> GetBatchBalanceAsync(long batchId)
        {
            // Calculate balance as Sum(In) - Sum(Out)
            // This is the safest way to ensure data integrity in an ERP
            return await _context.StockMovements
                .AsNoTracking()
                .Where(m => m.BatchId == (int)batchId)
                .SumAsync(m => m.QtyIn - m.QtyOut);
        }

        public async Task<decimal> GetItemLastCostAsync(int itemId)
        {
            // Get the cost from the latest purchase movement
            var lastPurchase = await _context.StockMovements
                .AsNoTracking()
                .Where(m => m.ItemId == itemId && m.QtyIn > 0 && m.RefType == StockRefType.Purchase)
                .OrderByDescending(m => m.Date)
                .ThenByDescending(m => m.Id)
                .FirstOrDefaultAsync();

            if (lastPurchase != null) return lastPurchase.UnitCost;

            // Fallback to Item's default purchase price
            var item = await _context.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == itemId);
            return item?.DefaultPurchasePrice ?? 0m;
        }

        public async Task<decimal> GetTotalInventoryValueAsync()
        {
            // Complex ERP logic: For each batch, get current balance and multiply by its specific cost
            // Or use latest item cost for all batches of that item. 
            // Here we use the batch's own purchase price for accuracy.
            var batches = await _context.ItemBatches
                .AsNoTracking()
                .Select(b => new {
                    b.Id,
                    b.PurchasePrice,
                    Balance = _context.StockMovements.Where(m => m.BatchId == b.Id).Sum(m => m.QtyIn - m.QtyOut)
                })
                .ToListAsync();

            return batches.Sum(b => b.Balance * b.PurchasePrice);
        }
    }
}
