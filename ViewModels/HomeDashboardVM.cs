namespace AbuAmenPharma.ViewModels
{
    public class HomeDashboardVM
    {
        public int SalesTodayCount { get; set; }
        public decimal SalesTodayNet { get; set; }

        public decimal SalesMonthNet { get; set; }

        public int UnpaidInvoicesCount { get; set; }
        public decimal UnpaidInvoicesRemaining { get; set; }

        public decimal ReceiptsTodayTotal { get; set; }

        public int ExpiredBatchesCount { get; set; }
        public int ExpiringSoonBatchesCount { get; set; } // خلال 30 يوم

        public int OutOfStockItemsCount { get; set; } // اختياري

        public List<TopItemVM> TopItems { get; set; } = new();
    }

    public class TopItemVM
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = "";
        public decimal Qty { get; set; }
    }
}
