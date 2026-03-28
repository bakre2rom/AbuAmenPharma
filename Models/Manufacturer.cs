using System.ComponentModel.DataAnnotations;

namespace AbuAmenPharma.Models
{
    public class Manufacturer
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string NameAr { get; set; } = string.Empty;

        [StringLength(60)]
        public string? Country { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
