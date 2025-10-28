using System.ComponentModel.DataAnnotations;

namespace SchoolLibrary.Web.Models
{
    public class SystemSettings
    {
        [Key]
        public int SettingID { get; set; }

        [Required]
        [StringLength(100)]
        public string SettingKey { get; set; } = string.Empty;

        [StringLength(500)]
        public string? SettingValue { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? SettingType { get; set; } // 'Integer', 'String', 'Boolean', 'Decimal'

        public DateTime? LastModified { get; set; }
    }
}


