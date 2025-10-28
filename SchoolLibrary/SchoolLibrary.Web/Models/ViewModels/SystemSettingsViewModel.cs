using System.ComponentModel.DataAnnotations;

namespace SchoolLibrary.Web.Models.ViewModels
{
    public class SystemSettingsViewModel
    {
        public List<SystemSettingItem> Settings { get; set; } = new List<SystemSettingItem>();
    }

    public class SystemSettingItem
    {
        public int SettingID { get; set; }

        [Required]
        public string SettingKey { get; set; } = string.Empty;

        [Display(Name = "Giá trị")]
        public string? SettingValue { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Loại")]
        public string? SettingType { get; set; }
    }
}


