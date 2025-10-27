using System.ComponentModel.DataAnnotations;

namespace SchoolLibrary.Web.Models
{
    public class Role
    {
        [Key]
        public int RoleID { get; set; }

        [Required]
        [StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}

