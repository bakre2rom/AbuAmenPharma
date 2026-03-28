using System.ComponentModel.DataAnnotations;

namespace AbuAmenPharma.Models
{
    public class Unit
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string NameAr { get; set; } = string.Empty;

        [StringLength(50)]
        public string? NameEn { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
