namespace AbuAmenPharma.ViewModels
{
    public class DataTablesRequest
    {
        public int draw { get; set; }
        public int start { get; set; }
        public int length { get; set; }

        public Search search { get; set; } = new();
        public List<Order> order { get; set; } = new();
        public List<Column> columns { get; set; } = new();

        public class Search { public string? value { get; set; } }
        public class Order { public int column { get; set; } public string? dir { get; set; } }
        public class Column { public string? data { get; set; } public string? name { get; set; } }
    }
}
