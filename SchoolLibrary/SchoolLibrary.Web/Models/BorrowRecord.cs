using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolLibrary.Web.Models
{
    public class BorrowRecord
    {
        [Key]
        public int BorrowID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int CopyID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime BorrowDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ReturnDate { get; set; }

        public bool IsReturned { get; set; } = false;

        [Column(TypeName = "decimal(10,2)")]
        public decimal FineAmount { get; set; } = 0;

        // Navigation properties
        [ForeignKey("UserID")]
        public virtual User? User { get; set; }

        [ForeignKey("CopyID")]
        public virtual BookCopy? BookCopy { get; set; }
    }
}

