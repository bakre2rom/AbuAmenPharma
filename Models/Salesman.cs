using System.ComponentModel.DataAnnotations;

namespace AbuAmenPharma.Models
{
    public class Salesman
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string NameAr { get; set; } = "";

        [StringLength(20)]
        public string? Phone { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    }

}
