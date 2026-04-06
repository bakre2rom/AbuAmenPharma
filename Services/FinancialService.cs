using AbuAmenPharma.Data;
using AbuAmenPharma.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace AbuAmenPharma.Services
{
    public class FinancialService : IFinancialService
    {
        private readonly ApplicationDbContext _context;

        public FinancialService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> GetCustomerDueBalanceAsync(int customerId)
        {
            // Centralized logic for due balance: sum of RemainingAmount from all posted sales
            return await _context.Sales
                .AsNoTracking()
                .Where(s => s.CustomerId == customerId && s.IsPosted && s.RemainingAmount > 0)
                .SumAsync(s => (decimal?)s.RemainingAmount) ?? 0m;
        }

        public async Task<decimal> AllocatePaymentAsync(long receiptId, int customerId, decimal totalAmount)
        {
            decimal remainingToAllocate = totalAmount;

            // Get oldest outstanding sales for this customer (FIFO)
            var targetSales = await _context.Sales
                .Where(s => s.CustomerId == customerId && s.IsPosted && s.RemainingAmount > 0)
                .OrderBy(s => s.SaleDate)
                .ThenBy(s => s.Id)
                .ToListAsync();

            foreach (var sale in targetSales)
            {
                if (remainingToAllocate <= 0) break;

                decimal unpaidOnSale = sale.RemainingAmount;
                if (unpaidOnSale <= 0) continue;

                decimal pay = Math.Min(unpaidOnSale, remainingToAllocate);

                // Create link between receipt and sale
                _context.CustomerReceiptAllocations.Add(new CustomerReceiptAllocation
                {
                    ReceiptId = receiptId,
                    SaleId = sale.Id,
                    Amount = pay
                });

                // Update Sale directly (DRY logic used in controllers now moved here)
                sale.PaidAmount += pay;
                sale.RemainingAmount = Math.Max(0, sale.NetTotal - sale.PaidAmount);

                remainingToAllocate -= pay;
            }

            return remainingToAllocate; // Returns unallocated credit
        }

        public async Task ReversePaymentAllocationAsync(long receiptId)
        {
            // Find all allocations for this receipt
            var allocations = await _context.CustomerReceiptAllocations
                .Include(a => a.Sale)
                .Where(a => a.ReceiptId == receiptId)
                .ToListAsync();

            foreach (var allocation in allocations)
            {
                if (allocation.Sale != null)
                {
                    // Restore original balance on the sale record
                    allocation.Sale.PaidAmount -= allocation.Amount;
                    allocation.Sale.RemainingAmount = Math.Max(0, allocation.Sale.NetTotal - allocation.Sale.PaidAmount);
                }
            }

            // Remove allocation records
            _context.CustomerReceiptAllocations.RemoveRange(allocations);
        }
    }
}
