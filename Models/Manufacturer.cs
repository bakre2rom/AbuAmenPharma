using System.ComponentModel.DataAnnotations;

namespace AbuAmenPharma.Models
{
    public class Manufacturer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "حقل الاسم مطلوب"), StringLength(100, ErrorMessage = "الحد الأقصى 100 حرفاً")]
        public string NameAr { get; set; } = string.Empty;

        [StringLength(60)]
        public string? Country { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
