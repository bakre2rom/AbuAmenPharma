using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AbuAmenPharma.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Customer name is required.")]
        [StringLength(100)]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Phone")]
        public string? Phone { get; set; }

        [StringLength(200)]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Balance")]
        public decimal Balance { get; set; }

        [Display(Name = "Salesman")]
        public int? SalesmanId { get; set; }
        
        [ForeignKey("SalesmanId")]
        public Salesman? Salesman { get; set; }
    }
}
