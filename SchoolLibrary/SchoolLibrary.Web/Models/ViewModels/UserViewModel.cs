using System.ComponentModel.DataAnnotations;

namespace SchoolLibrary.Web.Models.ViewModels
{
    public class UserViewModel
    {
        public int UserID { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = string.Empty;

        [Display(Name = "Mật khẩu")]
        public string? Password { get; set; }

        [Required]
        [Display(Name = "Vai trò")]
        public int RoleID { get; set; }

        [Display(Name = "Lớp/Tổ bộ môn")]
        public string? ClassOrDepartment { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [Display(Name = "Trạng thái")]
        public bool IsActive { get; set; } = true;

        // For display
        public string? RoleName { get; set; }
    }
}

