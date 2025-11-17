using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolLibrary.Web.Models
{
    public class BookSuggestion
    {
        [Key]
        public int SuggestionID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        [StringLength(255)]
        public string BookTitle { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Author { get; set; }

        [StringLength(20)]
        public string? ISBN { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        [DataType(DataType.Date)]
        public DateTime SuggestionDate { get; set; } = DateTime.Now;

        [DataType(DataType.Date)]
        public DateTime? ReviewedDate { get; set; }

        // Navigation properties
        [ForeignKey("UserID")]
        public virtual User? User { get; set; }
    }
}

