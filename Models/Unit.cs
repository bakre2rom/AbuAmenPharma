using System.ComponentModel.DataAnnotations;

namespace AbuAmenPharma.Models
{
    public class Unit
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "حقل الاسم مطلوب"), StringLength(50, ErrorMessage = "الحد الأقصى 50 حرفاً")]
        public string NameAr { get; set; } = string.Empty;

        [StringLength(50)]
        public string? NameArNormalized { get; set; }

        [StringLength(50)]
        public string? NameEn { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
