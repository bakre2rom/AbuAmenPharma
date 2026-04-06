namespace AbuAmenPharma.ViewModels
{
    public class ERPDataTableConfig
    {
        public string TableId { get; set; } = "erpTable";
        public string AjaxUrl { get; set; } = string.Empty;
        public string ColumnsJson { get; set; } = "[]";
        public int PageLength { get; set; } = 5;
        public string Order { get; set; } = "[[0, 'desc']]";
        
        // Optional: Extend with more settings if needed (Searchable, Exportable, etc.)
        public bool IsServerSide { get; set; } = true;
    }
}
