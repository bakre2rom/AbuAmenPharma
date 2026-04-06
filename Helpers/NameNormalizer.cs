namespace AbuAmenPharma.Helpers
{
    public static class NameNormalizer
    {
        public static string NormalizeForLookup(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value.Trim().ToUpperInvariant();
        }
    }
}
