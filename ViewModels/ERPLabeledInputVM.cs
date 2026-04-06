namespace AbuAmenPharma.ViewModels
{
    public class ERPLabeledInputVM
    {
        public string? Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string Type { get; set; } = "text";
        public bool Required { get; set; } = false;
        public string? Placeholder { get; set; }
        public string? CssClass { get; set; }
        public string? Attributes { get; set; }
        public bool Readonly { get; set; } = false;
    }
}
