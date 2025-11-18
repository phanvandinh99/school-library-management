using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolLibrary.Web.Models
{
    public class Author
    {
        [Key]
        public int AuthorID { get; set; }

        [Required]
        [StringLength(255)]
        public string AuthorName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Biography { get; set; }

        [StringLength(100)]
        public string? Nationality { get; set; }

        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation properties - Many-to-Many với Book
        public virtual ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
    }
}

