using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using SchoolLibrary.Web.Data;
using SchoolLibrary.Web.Models;

namespace SchoolLibrary.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class BackupController : Controller
    {
        private readonly LibraryDbContext _context;
        private readonly IConfiguration _configuration;

        public BackupController(LibraryDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        // GET: Index
        public IActionResult Index()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

        // POST: Create Backup
        [HttpPost]
        public async Task<IActionResult> CreateBackup()
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền" });
            }

            try
            {
                // Export data to JSON
                var backup = new
                {
                    Date = DateTime.Now,
                    Roles = await _context.Roles.ToListAsync(),
                    Users = await _context.Users.ToListAsync(),
                    Categories = await _context.Categories.ToListAsync(),
                    Books = await _context.Books.ToListAsync(),
                    BookCopies = await _context.BookCopies.ToListAsync(),
                    BorrowRecords = await _context.BorrowRecords.ToListAsync(),
                    SystemSettings = await _context.SystemSettings.ToListAsync()
                };

                var json = System.Text.Json.JsonSerializer.Serialize(backup, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

                var fileName = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "backups", fileName);

                // Create directory if not exists
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                await System.IO.File.WriteAllTextAsync(path, json);

                TempData["SuccessMessage"] = $"Sao lưu thành công! File: {fileName}";
                return Json(new { success = true, message = $"Sao lưu thành công! File: {fileName}", fileName = fileName });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi sao lưu: {ex.Message}";
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // GET: Download Backup
        public IActionResult DownloadBackup(string fileName)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "backups", fileName);
            
            if (!System.IO.File.Exists(path))
            {
                TempData["ErrorMessage"] = "File không tồn tại";
                return RedirectToAction(nameof(Index));
            }

            var fileBytes = System.IO.File.ReadAllBytes(path);
            return File(fileBytes, "application/json", fileName);
        }

        // GET: List Backups
        public IActionResult ListBackups()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var backupPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "backups");
            
            if (!Directory.Exists(backupPath))
            {
                return Json(new List<object>());
            }

            var files = Directory.GetFiles(backupPath, "backup_*.json")
                .Select(f => new
                {
                    FileName = Path.GetFileName(f),
                    Size = new FileInfo(f).Length,
                    CreatedDate = System.IO.File.GetCreationTime(f)
                })
                .OrderByDescending(f => f.CreatedDate)
                .ToList();

            return Json(files);
        }

        // POST: Restore Backup
        [HttpPost]
        public async Task<IActionResult> RestoreBackup(string fileName)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền" });
            }

            try
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "backups", fileName);
                
                if (!System.IO.File.Exists(path))
                {
                    return Json(new { success = false, message = "File không tồn tại" });
                }

                var json = await System.IO.File.ReadAllTextAsync(path);
                var backup = System.Text.Json.JsonSerializer.Deserialize<BackupData>(json);

                // Clear existing data
                _context.BorrowRecords.RemoveRange(await _context.BorrowRecords.ToListAsync());
                _context.BookCopies.RemoveRange(await _context.BookCopies.ToListAsync());
                _context.Books.RemoveRange(await _context.Books.ToListAsync());
                _context.Categories.RemoveRange(await _context.Categories.ToListAsync());
                _context.Users.RemoveRange(await _context.Users.ToListAsync());
                _context.Roles.RemoveRange(await _context.Roles.ToListAsync());
                _context.SystemSettings.RemoveRange(await _context.SystemSettings.ToListAsync());

                await _context.SaveChangesAsync();

                // Restore data
                await _context.Roles.AddRangeAsync(backup.Roles);
                await _context.Users.AddRangeAsync(backup.Users);
                await _context.Categories.AddRangeAsync(backup.Categories);
                await _context.Books.AddRangeAsync(backup.Books);
                await _context.BookCopies.AddRangeAsync(backup.BookCopies);
                await _context.BorrowRecords.AddRangeAsync(backup.BorrowRecords);
                await _context.SystemSettings.AddRangeAsync(backup.SystemSettings);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Phục hồi dữ liệu thành công!";
                return Json(new { success = true, message = "Phục hồi thành công" });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi phục hồi: {ex.Message}";
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }

    // Helper class for backup data
    public class BackupData
    {
        public DateTime Date { get; set; }
        public List<Role> Roles { get; set; } = new();
        public List<User> Users { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Book> Books { get; set; } = new();
        public List<BookCopy> BookCopies { get; set; } = new();
        public List<BorrowRecord> BorrowRecords { get; set; } = new();
        public List<SystemSettings> SystemSettings { get; set; } = new();
    }
}

