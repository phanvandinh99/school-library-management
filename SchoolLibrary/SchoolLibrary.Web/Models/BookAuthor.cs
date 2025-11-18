using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolLibrary.Web.Models
{
    // Bảng trung gian cho quan hệ Many-to-Many giữa Book và Author
    public class BookAuthor
    {
        [Key]
        public int BookAuthorID { get; set; }

        [Required]
        public int BookID { get; set; }

        [Required]
        public int AuthorID { get; set; }

        // Navigation properties
        [ForeignKey("BookID")]
        public virtual Book? Book { get; set; }

        [ForeignKey("AuthorID")]
        public virtual Author? Author { get; set; }
    }
}

