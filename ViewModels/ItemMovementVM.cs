namespace AbuAmenPharma.ViewModels
{
    public class ItemMovementVM
    {
        public DateTime Date { get; set; }
        public string RefType { get; set; } = "";
        public long RefId { get; set; }

        public string? BatchNo { get; set; }

        public decimal QtyIn { get; set; }
        public decimal QtyOut { get; set; }

        public decimal BalanceAfter { get; set; }
    }
}
