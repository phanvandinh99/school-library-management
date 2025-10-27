using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolLibrary.Web.Models
{
    public class BookCopy
    {
        [Key]
        public int CopyID { get; set; }

        [Required]
        public int BookID { get; set; }

        [Required]
        [StringLength(20)]
        public string CopyCode { get; set; } = string.Empty;

        [StringLength(20)]
        public string Status { get; set; } = "Available";

        // Navigation properties
        [ForeignKey("BookID")]
        public virtual Book? Book { get; set; }

        public virtual ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();
    }
}

