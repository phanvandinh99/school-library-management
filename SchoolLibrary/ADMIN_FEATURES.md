# Tính năng Admin - Hệ thống Quản lý Thư viện

## ✅ Hoàn thành

### 1. 🏠 Dashboard
- **Hiển thị tổng quan**: Tổng người dùng, sách, sách đang mượn, thể loại
- **Thống kê user**: Số người hoạt động / không hoạt động
- **Người dùng mới**: Top 5 người dùng mới nhất
- **Mượn sách gần đây**: Lịch sử mượn sách mới nhất
- **Thời gian thực**: Hiển thị ngày hiện tại

### 2. 👥 Quản lý Người dùng
- **Danh sách**: Hiển thị tất cả người dùng với thông tin chi tiết
- **Thêm mới**: Tạo tài khoản với đầy đủ thông tin
- **Chỉnh sửa**: Cập nhật thông tin và đổi mật khẩu
- **Xóa**: Xóa người dùng khỏi hệ thống
- **Tìm kiếm**: Đang phát triển thêm

### 3. ⚙️ Cấu hình Hệ thống
- **Thời hạn mượn mặc định**: 14 ngày
- **Số ngày gia hạn tối đa**: 7 ngày
- **Tiền phạt**: 5000 VNĐ/ngày
- **Số sách mượn tối đa**: 5 cuốn
- **Ngày gia hạn không phạt**: 3 ngày
- **Tên hệ thống**: Có thể tùy chỉnh
- **Nhắc nhở**: 3 ngày trước khi hết hạn

**Các cài đặt có thể điều chỉnh trực tiếp trên giao diện!**

### 4. 📊 Giám sát Hoạt động
- **Thống kê theo thời gian**: Hôm nay, tuần này, tháng này
- **Sách quá hạn**: Hiển thị và cảnh báo sách quá hạn
- **Hoạt động mượn**: Lịch sử mượn sách trong tuần
- **Thống kê nhanh**: Mượn, trả, đang mượn, có phạt
- **Danh sách chi tiết**: Thông tin đầy đủ mỗi lượt mượn

### 5. 📈 Báo cáo & Thống kê

#### Báo cáo Tuần
- Thống kê mượn/trả trong tuần
- Top sách được mượn nhiều nhất
- Chi tiết từng lượt mượn

#### Báo cáo Tháng
- Thống kê theo tháng (có thể chọn tháng/năm)
- Hoạt động người dùng
- Top 20 người dùng hoạt động

#### Báo cáo Năm
- Tổng quan theo năm
- Thống kê theo từng tháng
- Xu hướng trong năm

#### Tổng quan Thống kê
- Tổng số: Người dùng, sách, lượt mượn, thể loại
- Phân loại sách theo thể loại
- Phân loại người dùng theo vai trò

### 6. 💾 Sao lưu & Phục hồi
- **Tạo backup**: Xuất toàn bộ dữ liệu ra file JSON
- **Danh sách backup**: Xem tất cả file đã sao lưu
- **Tải xuống**: Download file backup về máy
- **Phục hồi**: Khôi phục dữ liệu từ file backup
- **Cảnh báo**: Thông báo rõ ràng trước khi phục hồi

## 🎨 Giao diện

### Layout Admin
- **Sidebar cố định**: Menu bên trái với gradient đẹp mắt
- **Active state**: Highlight menu đang chọn
- **Responsive**: Tự động điều chỉnh trên mobile
- **User info**: Hiển thị thông tin người dùng đang đăng nhập
- **Logout**: Nút đăng xuất ngay trong sidebar

### Style System
- **Card-based**: Tất cả nội dung trong card trắng
- **Color-coded**: Màu sắc phân biệt theo loại
- **Icons**: Emoji để dễ nhận biết
- **Badges**: Badge Bootstrap cho trạng thái
- **Tables**: Bảng responsive với hover effect

## 🔐 Bảo mật

- **Session-based authentication**: Đăng nhập bằng session
- **Role-based authorization**: Kiểm tra quyền Admin
- **Password hashing**: SHA256 hash
- **CSRF protection**: Antiforgery token
- **Session timeout**: 30 phút

## 📍 Routes Admin

```
/Admin/Auth/Login              - Đăng nhập
/Admin/Admin/Dashboard         - Dashboard
/Admin/UserManagement/Index    - Danh sách người dùng
/Admin/UserManagement/Create    - Thêm người dùng
/Admin/UserManagement/Edit/{id} - Sửa người dùng
/Admin/UserManagement/Delete/{id} - Xóa người dùng
/Admin/SystemConfiguration/Index - Cấu hình hệ thống
/Admin/ActivityMonitor/Index   - Giám sát hoạt động
/Admin/ActivityMonitor/OverdueBooks - Sách quá hạn
/Admin/Reports/Index           - Báo cáo
/Admin/Reports/WeeklyReport     - Báo cáo tuần
/Admin/Reports/MonthlyReport   - Báo cáo tháng
/Admin/Reports/YearlyReport    - Báo cáo năm
/Admin/Reports/StatisticsSummary - Tổng quan thống kê
/Admin/Backup/Index            - Sao lưu & Phục hồi
```

## 🔄 Migration

Sau khi cập nhật Database, chạy script:
```sql
-- Database/AddSystemSettingsTable.sql
```

Hoặc dùng Entity Framework:
```bash
dotnet ef migrations add AddSystemSettings
dotnet ef database update
```

## 📝 Các tính năng tiếp theo có thể phát triển

- [ ] Thông báo email cho sách quá hạn
- [ ] QR code cho sách
- [ ] Mã vạch scanner
- [ ] Tìm kiếm nâng cao với filters
- [ ] Export Excel/PDF báo cáo
- [ ] Logs chi tiết hoạt động
- [ ] Audit trail
- [ ] Backup tự động định kỳ

## 🚀 Hướng dẫn sử dụng

Xem file `HUONG_DAN_SU_DUNG.md` để biết cách cài đặt và sử dụng!

