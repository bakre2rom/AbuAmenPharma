using System.Threading.Tasks;

namespace AbuAmenPharma.Services
{
    public interface IFinancialService
    {
        /// <summary>
        /// Calculates the total due balance for a customer from all posted, unpaid sales invoices.
        /// </summary>
        Task<decimal> GetCustomerDueBalanceAsync(int customerId);

        /// <summary>
        /// Allocates a receipt amount across the oldest outstanding sales invoices for a customer (FIFO).
        /// </summary>
        Task<decimal> AllocatePaymentAsync(long receiptId, int customerId, decimal totalAmount);

        /// <summary>
        /// Reverses allocations and updates sale balances when a receipt is disabled/deleted.
        /// </summary>
        Task ReversePaymentAllocationAsync(long receiptId);
    }
}
