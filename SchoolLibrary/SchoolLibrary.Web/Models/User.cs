using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolLibrary.Web.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public int RoleID { get; set; }

        [StringLength(50)]
        public string? ClassOrDepartment { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(15)]
        public string? Phone { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("RoleID")]
        public virtual Role? Role { get; set; }

        public virtual ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();
    }
}

