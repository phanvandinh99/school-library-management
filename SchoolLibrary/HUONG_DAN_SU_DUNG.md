# Hướng dẫn sử dụng Hệ thống Quản lý Thư viện

## 🚀 Cài đặt và Chạy ứng dụng

### Bước 1: Chạy Script Database
1. Mở SQL Server Management Studio
2. Kết nối với SQL Server (LocalDB hoặc SQL Server Express)
3. Chạy file `Database/LibraryDatabase.sql` để tạo database và dữ liệu mẫu

### Bước 1.1: Cập nhật Password Hash
1. Chạy ứng dụng (với dotnet run)
2. Truy cập: `https://localhost:XXXX/Admin/Helper/GenerateHash?password=123456`
3. Copy hash được tạo
4. Mở SQL Server Management Studio
5. Chạy file `Database/UpdatePasswordHashes.sql`
6. Thay `YOUR_HASH_HERE` bằng hash vừa copy
7. Chạy script để cập nhật mật khẩu

### Bước 2: Cấu hình Connection String
Nếu database của bạn khác với LocalDB, sửa file `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=LibraryTHPT;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

### Bước 3: Restore Packages
```bash
cd SchoolLibrary/SchoolLibrary.Web
dotnet restore
```

### Bước 4: Chạy ứng dụng
```bash
dotnet run
```
Hoặc nhấn F5 trong Visual Studio

## 👤 Tài khoản mặc định

**Sau khi cập nhật password hash**, bạn có thể đăng nhập với:

- **Admin**: 
  - Username: `admin`
  - Password: `123456`

⚠️ **Quan trọng**: Bạn phải chạy `Database/UpdatePasswordHashes.sql` sau khi chạy `LibraryDatabase.sql` để có thể đăng nhập được!

### Các tài khoản khác:
- **Học sinh**: `phana` / `123456`
- **Giáo viên**: `nguyenb` / `123456`
- **Thủ thư**: `thuthu` / `123456`

## 📋 Chức năng Admin

### 1. Đăng nhập / Đăng xuất
- URL: `/Admin/Auth/Login`
- Có validation form
- Session timeout: 30 phút

### 2. Dashboard
- Hiển thị thống kê: Tổng người dùng, sách, sách đang mượn, thể loại
- Thống kê người dùng hoạt động
- Danh sách người dùng mới nhất

### 3. Quản lý Người dùng
- **Xem danh sách**: Hiển thị tất cả người dùng với thông tin chi tiết
- **Thêm mới**: Tạo tài khoản người dùng mới với các vai trò (Học sinh, Giáo viên, Thủ thư, Admin)
- **Sửa**: Cập nhật thông tin người dùng, bao gồm đổi mật khẩu
- **Xóa**: Xóa người dùng khỏi hệ thống

### Cấu trúc Routes Admin
- Đăng nhập: `/Admin/Auth/Login`
- Dashboard: `/Admin/Admin/Dashboard`
- Quản lý User: `/Admin/UserManagement/Index`
- Thêm User: `/Admin/UserManagement/Create`
- Sửa User: `/Admin/UserManagement/Edit/{id}`
- Xóa User: `/Admin/UserManagement/Delete/{id}`

## 🔐 Bảo mật

- Session-based authentication
- Password hashing (SHA256)
- Role-based authorization
- CSRF protection với AntiforgeryToken

## 📝 Ghi chú

- Mật khẩu mặc định cho người dùng mới: `123456`
- Khi thêm người dùng mới, để trống mật khẩu sẽ dùng mật khẩu mặc định
- Khi sửa người dùng, để trống mật khẩu sẽ giữ nguyên mật khẩu cũ

## 🎯 Các bước tiếp theo

Chức năng đã hoàn thành:
- ✅ Đăng nhập/Đăng xuất
- ✅ Dashboard Admin
- ✅ Quản lý người dùng (CRUD)
- ✅ Phân quyền theo vai trò

Chức năng cần phát triển tiếp:
- ⏳ Quản lý sách và danh mục
- ⏳ Quản lý mượn trả sách
- ⏳ Cấu hình hệ thống
- ⏳ Báo cáo thống kê
- ⏳ Các chức năng cho Thủ thư, Giáo viên, Học sinh

