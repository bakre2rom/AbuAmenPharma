using System.Threading.Tasks;
using System.Collections.Generic;
using AbuAmenPharma.Models;

namespace AbuAmenPharma.Services
{
    public enum StockMovementType
    {
        In,
        Out
    }

    public interface IStockService
    {
        /// <summary>
        /// Updates the balance of a specific batch and records the stock movement.
        /// Handles both negative (Sales/Returns to supplier) and positive (Purchases/Customer Returns) movements.
        /// </summary>
        Task<bool> UpdateStockAsync(long batchId, decimal quantity, StockMovementType type, string refNo, string notes = null);

        /// <summary>
        /// Gets the current available balance for a specific item batch.
        /// </summary>
        Task<decimal> GetBatchBalanceAsync(long batchId);

        /// <summary>
        /// Gets the latest purchase cost for an item to calculate inventory valuation.
        /// </summary>
        Task<decimal> GetItemLastCostAsync(int itemId);

        /// <summary>
        /// Calculates the total valuation of the current inventory (Sum of Balance * Last Cost).
        /// </summary>
        Task<decimal> GetTotalInventoryValueAsync();
    }
}
