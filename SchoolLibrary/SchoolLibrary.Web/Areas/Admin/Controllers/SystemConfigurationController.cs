using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using SchoolLibrary.Web.Data;
using SchoolLibrary.Web.Models;
using SchoolLibrary.Web.Models.ViewModels;

namespace SchoolLibrary.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SystemConfigurationController : Controller
    {
        private readonly LibraryDbContext _context;

        public SystemConfigurationController(LibraryDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        // GET: Index
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            // Get all settings or create defaults if none exist
            var settings = await _context.SystemSettings.ToListAsync();
            
            if (!settings.Any())
            {
                // Create default settings
                var defaultSettings = new List<SystemSettings>
                {
                    new SystemSettings 
                    { 
                        SettingKey = "DefaultBorrowDays", 
                        SettingValue = "14", 
                        Description = "Số ngày mượn mặc định",
                        SettingType = "Integer"
                    },
                    new SystemSettings 
                    { 
                        SettingKey = "MaxRenewDays", 
                        SettingValue = "7", 
                        Description = "Số ngày gia hạn tối đa",
                        SettingType = "Integer"
                    },
                    new SystemSettings 
                    { 
                        SettingKey = "FinePerDay", 
                        SettingValue = "5000", 
                        Description = "Tiền phạt mỗi ngày trễ hạn (VNĐ)",
                        SettingType = "Integer"
                    },
                    new SystemSettings 
                    { 
                        SettingKey = "MaxBorrowBooks", 
                        SettingValue = "5", 
                        Description = "Số sách mượn tối đa mỗi người",
                        SettingType = "Integer"
                    },
                    new SystemSettings 
                    { 
                        SettingKey = "GracePeriod", 
                        SettingValue = "3", 
                        Description = "Số ngày gia hạn không phạt",
                        SettingType = "Integer"
                    },
                    new SystemSettings 
                    { 
                        SettingKey = "SystemName", 
                        SettingValue = "Thư viện Trường THPT", 
                        Description = "Tên hệ thống",
                        SettingType = "String"
                    },
                    new SystemSettings 
                    { 
                        SettingKey = "LateNotificationDays", 
                        SettingValue = "3", 
                        Description = "Nhắc nhở trước khi hết hạn (ngày)",
                        SettingType = "Integer"
                    }
                };

                _context.SystemSettings.AddRange(defaultSettings);
                await _context.SaveChangesAsync();

                settings = await _context.SystemSettings.ToListAsync();
            }

            var viewModel = new SystemSettingsViewModel
            {
                Settings = settings.Select(s => new SystemSettingItem
                {
                    SettingID = s.SettingID,
                    SettingKey = s.SettingKey,
                    SettingValue = s.SettingValue,
                    Description = s.Description,
                    SettingType = s.SettingType
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Update Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSettings(SystemSettingsViewModel model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                foreach (var item in model.Settings)
                {
                    var setting = await _context.SystemSettings.FindAsync(item.SettingID);
                    if (setting != null)
                    {
                        setting.SettingValue = item.SettingValue;
                        setting.LastModified = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật cấu hình hệ thống thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Reset to defaults
        public async Task<IActionResult> ResetDefaults()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var settings = await _context.SystemSettings.ToListAsync();
                _context.SystemSettings.RemoveRange(settings);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã reset về cấu hình mặc định!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi reset: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

