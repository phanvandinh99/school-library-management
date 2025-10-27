# Hệ thống Quản lý Thư viện Trường THPT

Hệ thống quản lý thư viện trường học với phân quyền theo vai trò (Học sinh, Giáo viên, Thủ thư, Admin).

## 📋 Tổng quan chức năng

### 🔐 Admin / Quản trị viên
- ✅ Đăng nhập / Đăng xuất
- ✅ Quản lý tài khoản người dùng (CRUD)
- ✅ Phân quyền vai trò
- ⏳ Cấu hình hệ thống
- ⏳ Sao lưu & phục hồi dữ liệu
- ⏳ Giám sát hoạt động hệ thống

### 📚 Thủ thư / Nhân viên thư viện
- ⏳ Đăng nhập / Đăng xuất
- ⏳ Quản lý sách (CRUD)
- ⏳ Quản lý danh mục
- ⏳ Xử lý mượn/trả sách
- ⏳ Gia hạn mượn
- ⏳ Báo cáo thống kê

### 👨‍🎓 Học sinh và Giáo viên
- ⏳ Đăng nhập / Đăng xuất
- ⏳ Tra cứu sách
- ⏳ Xem lịch sử mượn
- ⏳ Đặt trước sách
- ⏳ Đề xuất mua sách

## 🛠️ Tech Stack

- **Framework**: ASP.NET Core 8.0 (MVC)
- **Database**: SQL Server / LocalDB
- **ORM**: Entity Framework Core
- **Frontend**: Bootstrap 5, jQuery
- **Authentication**: Session-based

## 📁 Cấu trúc Project

```
SchoolLibrary.Web/
├── Areas/
│   ├── Admin/        # Khu vực Admin (✅ Hoàn thành)
│   ├── Librarian/    # Khu vực Thủ thư (⏳ Chưa phát triển)
│   ├── Student/      # Khu vực Học sinh (⏳ Chưa phát triển)
│   └── Teacher/      # Khu vực Giáo viên (⏳ Chưa phát triển)
├── Models/           # Database Models
├── ViewModels/       # View Models
├── Data/             # DbContext
├── Services/         # Business Logic Services
└── wwwroot/          # Static files
```

## 🚀 Hướng dẫn cài đặt

Xem file [HUONG_DAN_SU_DUNG.md](./HUONG_DAN_SU_DUNG.md) để biết chi tiết.

### Tóm tắt nhanh:
1. Chạy `Database/LibraryDatabase.sql`
2. Generate password hash và cập nhật database
3. Configure connection string trong `appsettings.json`
4. `dotnet run`

## 📝 Database Schema

- **Roles**: Vai trò người dùng
- **Users**: Thông tin người dùng
- **Categories**: Thể loại sách
- **Books**: Đầu sách
- **BookCopies**: Bản sao sách vật lý
- **BorrowRecords**: Ghi chép mượn trả

## 🔑 Quyền truy cập

### Vai trò trong hệ thống:
1. **Học sinh** (RoleID=1)
2. **Giáo viên** (RoleID=2)
3. **Thủ thư** (RoleID=3)
4. **Admin** (RoleID=4)

## 📊 Trạng thái phát triển

- ✅ Database Schema
- ✅ Authentication/Authorization
- ✅ Admin: User Management
- ⏳ Admin: System Configuration
- ⏳ Librarian: Book Management
- ⏳ Librarian: Borrow/Return Management
- ⏳ User: Book Search
- ⏳ User: Borrow History
- ⏳ Reports & Statistics

## 👥 Tác giả

Hệ thống Quản lý Thư viện THPT

## 📄 License

MIT License

