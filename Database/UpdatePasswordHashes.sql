-- Script này cập nhật mật khẩu hash cho các tài khoản mẫu
-- Mật khẩu gốc: 123456
-- Chạy helper /Admin/Helper/GenerateHash?password=123456 để lấy hash và paste vào đây

USE LibraryTHPT;
GO

-- Cập nhật mật khẩu cho tất cả user
-- Thay YOUR_HASH_HERE bằng kết quả từ GenerateHash endpoint
UPDATE Users 
SET PasswordHash = 'YOUR_HASH_HERE'
WHERE Username IN ('admin', 'phana', 'nguyenb', 'thuthu');
GO

-- Hoặc bạn có thể chạy từng lệnh riêng:
-- UPDATE Users SET PasswordHash = 'YOUR_HASH_HERE' WHERE Username = 'admin';
-- UPDATE Users SET PasswordHash = 'YOUR_HASH_HERE' WHERE Username = 'phana';
-- UPDATE Users SET PasswordHash = 'YOUR_HASH_HERE' WHERE Username = 'nguyenb';
-- UPDATE Users SET PasswordHash = 'YOUR_HASH_HERE' WHERE Username = 'thuthu';

PRINT 'Đã cập nhật mật khẩu cho tất cả người dùng!';
GO

