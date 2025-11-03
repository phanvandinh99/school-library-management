using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using SchoolLibrary.Web.Data;
using SchoolLibrary.Web.Models;
using SchoolLibrary.Web.Models.ViewModels;
using SchoolLibrary.Web.Services;

namespace SchoolLibrary.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserManagementController : Controller
    {
        private readonly LibraryDbContext _context;

        public UserManagementController(LibraryDbContext context)
        {
            _context = context;
        }

        // Kiểm tra quyền Admin
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        // GET: User Management Index
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var users = await _context.Users
                .Include(u => u.Role)
                .Select(u => new UserViewModel
                {
                    UserID = u.UserID,
                    FullName = u.FullName,
                    Username = u.Username,
                    Email = u.Email,
                    Phone = u.Phone,
                    RoleID = u.RoleID,
                    RoleName = u.Role!.RoleName,
                    ClassOrDepartment = u.ClassOrDepartment,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            return View(users);
        }

        // GET: Create
        public async Task<IActionResult> Create()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "RoleID", "RoleName");
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "RoleID", "RoleName", model.RoleID);
                return View(model);
            }

            // Check if username exists
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "RoleID", "RoleName", model.RoleID);
                return View(model);
            }

            var user = new User
            {
                FullName = model.FullName,
                Username = model.Username,
                PasswordHash = HashService.HashPassword(model.Password ?? "123456"), // Default password
                RoleID = model.RoleID,
                ClassOrDepartment = model.ClassOrDepartment,
                Email = model.Email,
                Phone = model.Phone,
                IsActive = model.IsActive
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thêm người dùng thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null)
            {
                return NotFound();
            }

            var model = new UserViewModel
            {
                UserID = user.UserID,
                FullName = user.FullName,
                Username = user.Username,
                Email = user.Email,
                Phone = user.Phone,
                RoleID = user.RoleID,
                ClassOrDepartment = user.ClassOrDepartment,
                IsActive = user.IsActive
            };

            ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "RoleID", "RoleName", model.RoleID);
            return View(model);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserViewModel model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (id != model.UserID)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "RoleID", "RoleName", model.RoleID);
                return View(model);
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Check username duplicate
            if (await _context.Users.AnyAsync(u => u.Username == model.Username && u.UserID != id))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "RoleID", "RoleName", model.RoleID);
                return View(model);
            }

            user.FullName = model.FullName;
            user.Username = model.Username;
            user.RoleID = model.RoleID;
            user.ClassOrDepartment = model.ClassOrDepartment;
            user.Email = model.Email;
            user.Phone = model.Phone;
            user.IsActive = model.IsActive;

            // Only update password if provided
            if (!string.IsNullOrEmpty(model.Password))
            {
                user.PasswordHash = HashService.HashPassword(model.Password);
            }

            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await UserExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            TempData["SuccessMessage"] = "Cập nhật người dùng thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Xóa người dùng thành công!";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> UserExists(int id)
        {
            return await _context.Users.AnyAsync(e => e.UserID == id);
        }
    }
}

