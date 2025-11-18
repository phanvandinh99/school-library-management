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
        public string? Author { get; set; } // Giữ lại để backward compatibility

        [StringLength(20)]
        public string? ISBN { get; set; }

        [StringLength(100)]
        public string? Publisher { get; set; } // Giữ lại để backward compatibility

        public int? PublisherID { get; set; } // Foreign key đến Publisher

        public int? PublishYear { get; set; }

        public int? CategoryID { get; set; }

        public int TotalCopies { get; set; } = 1;

        // Navigation properties
        [ForeignKey("CategoryID")]
        public virtual Category? Category { get; set; }

        [ForeignKey("PublisherID")]
        public virtual Publisher? PublisherEntity { get; set; }

        // Many-to-Many với Author qua BookAuthor
        public virtual ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();

        public virtual ICollection<BookCopy> BookCopies { get; set; } = new List<BookCopy>();
    }
}

