using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolLibrary.Web.Models
{
    public class Book
    {
        [Key]
        public int BookID { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Author { get; set; }

        [StringLength(20)]
        public string? ISBN { get; set; }

        [StringLength(100)]
        public string? Publisher { get; set; }

        public int? PublishYear { get; set; }

        public int? CategoryID { get; set; }

        public int TotalCopies { get; set; } = 1;

        // Navigation properties
        [ForeignKey("CategoryID")]
        public virtual Category? Category { get; set; }

        public virtual ICollection<BookCopy> BookCopies { get; set; } = new List<BookCopy>();
    }
}

