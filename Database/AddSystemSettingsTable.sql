-- Script thêm bảng SystemSettings vào database

USE LibraryTHPT;
GO

-- Tạo bảng SystemSettings nếu chưa tồn tại
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SystemSettings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SystemSettings] (
        [SettingID] INT PRIMARY KEY IDENTITY(1,1),
        [SettingKey] NVARCHAR(100) NOT NULL UNIQUE,
        [SettingValue] NVARCHAR(500),
        [Description] NVARCHAR(200),
        [SettingType] NVARCHAR(100),
        [LastModified] DATETIME
    );

    -- Chèn dữ liệu mặc định
    INSERT INTO SystemSettings (SettingKey, SettingValue, Description, SettingType, LastModified)
    VALUES
    (N'DefaultBorrowDays', '14', N'Số ngày mượn mặc định', 'Integer', GETDATE()),
    (N'MaxRenewDays', '7', N'Số ngày gia hạn tối đa', 'Integer', GETDATE()),
    (N'FinePerDay', '5000', N'Tiền phạt mỗi ngày trễ hạn (VNĐ)', 'Integer', GETDATE()),
    (N'MaxBorrowBooks', '5', N'Số sách mượn tối đa mỗi người', 'Integer', GETDATE()),
    (N'GracePeriod', '3', N'Số ngày gia hạn không phạt', 'Integer', GETDATE()),
    (N'SystemName', N'Thư viện Trường THPT', N'Tên hệ thống', 'String', GETDATE()),
    (N'LateNotificationDays', '3', N'Nhắc nhở trước khi hết hạn (ngày)', 'Integer', GETDATE());

    PRINT 'Đã tạo bảng SystemSettings và chèn dữ liệu mặc định!';
END
ELSE
BEGIN
    PRINT 'Bảng SystemSettings đã tồn tại.';
END
GO

