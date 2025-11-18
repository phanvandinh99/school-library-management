-- =====================
-- FULL SETUP SCRIPT - CÓ THỂ CHẠY LẠI NHIỀU LẦN
-- Xóa database cũ và tạo mới với tất cả bảng + dữ liệu mẫu
-- =====================

USE master;
GO
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'LibraryTHPT')
BEGIN
    ALTER DATABASE LibraryTHPT SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE LibraryTHPT;
END
GO

CREATE DATABASE LibraryTHPT;
GO

USE LibraryTHPT;
GO

-- Tạo các bảng
CREATE TABLE Roles (
    RoleID INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    RoleID INT NOT NULL,
    ClassOrDepartment NVARCHAR(50),
    Email NVARCHAR(100),
    Phone NVARCHAR(15),
    IsActive BIT DEFAULT 1,
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);

CREATE TABLE Categories (
    CategoryID INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE Publishers (
    PublisherID INT PRIMARY KEY IDENTITY(1,1),
    PublisherName NVARCHAR(255) NOT NULL,
    Address NVARCHAR(500),
    Email NVARCHAR(100),
    Phone NVARCHAR(20),
    Description NVARCHAR(500)
);

CREATE TABLE Authors (
    AuthorID INT PRIMARY KEY IDENTITY(1,1),
    AuthorName NVARCHAR(255) NOT NULL,
    Biography NVARCHAR(500),
    Nationality NVARCHAR(100),
    BirthDate DATE,
    Description NVARCHAR(500)
);

CREATE TABLE Books (
    BookID INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(255) NOT NULL,
    Author NVARCHAR(255), -- Giữ lại để backward compatibility
    ISBN NVARCHAR(20),
    Publisher NVARCHAR(100), -- Giữ lại để backward compatibility
    PublisherID INT, -- Foreign key đến Publishers
    PublishYear INT,
    CategoryID INT,
    TotalCopies INT DEFAULT 1,
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID),
    FOREIGN KEY (PublisherID) REFERENCES Publishers(PublisherID)
);

CREATE TABLE BookAuthors (
    BookAuthorID INT PRIMARY KEY IDENTITY(1,1),
    BookID INT NOT NULL,
    AuthorID INT NOT NULL,
    FOREIGN KEY (BookID) REFERENCES Books(BookID) ON DELETE CASCADE,
    FOREIGN KEY (AuthorID) REFERENCES Authors(AuthorID) ON DELETE CASCADE,
    UNIQUE(BookID, AuthorID) -- Đảm bảo không có duplicate
);

CREATE TABLE BookCopies (
    CopyID INT PRIMARY KEY IDENTITY(1,1),
    BookID INT NOT NULL,
    CopyCode NVARCHAR(20) NOT NULL UNIQUE,
    Status NVARCHAR(20) DEFAULT 'Available',
    FOREIGN KEY (BookID) REFERENCES Books(BookID)
);

CREATE TABLE BorrowRecords (
    BorrowID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    CopyID INT NOT NULL,
    BorrowDate DATE NOT NULL,
    DueDate DATE NOT NULL,
    ReturnDate DATE NULL,
    IsReturned BIT DEFAULT 0,
    FineAmount DECIMAL(10,2) DEFAULT 0,
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (CopyID) REFERENCES BookCopies(CopyID)
);

CREATE TABLE SystemSettings (
    SettingID INT PRIMARY KEY IDENTITY(1,1),
    SettingKey NVARCHAR(100) NOT NULL UNIQUE,
    SettingValue NVARCHAR(500) NULL,
    Description NVARCHAR(200) NULL,
    SettingType NVARCHAR(100) NULL,
    LastModified DATETIME NULL
);

CREATE TABLE Reservations (
    ReservationID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    BookID INT NOT NULL,
    ReservationDate DATE NOT NULL,
    ExpiryDate DATE NULL,
    Status NVARCHAR(20) DEFAULT 'Pending',
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (BookID) REFERENCES Books(BookID)
);

CREATE TABLE BookSuggestions (
    SuggestionID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    BookTitle NVARCHAR(255) NOT NULL,
    Author NVARCHAR(255) NULL,
    ISBN NVARCHAR(20) NULL,
    Reason NVARCHAR(500) NULL,
    Status NVARCHAR(20) DEFAULT 'Pending',
    SuggestionDate DATE NOT NULL DEFAULT GETDATE(),
    ReviewedDate DATE NULL,
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

-- Dữ liệu mẫu
INSERT INTO Roles (RoleName) VALUES 
(N'Học sinh'), (N'Giáo viên'), (N'Thủ thư'), (N'Admin');

INSERT INTO Users (FullName, Username, PasswordHash, RoleID, ClassOrDepartment, Email, Phone) VALUES
(N'Phan Văn A', 'phana', 'RNuUoxlFpLzHh0ICDUCxGazq95emqPjGUGVskxvxN10=', 1, '11A1', 'phana@school.edu.vn', '0901234567'),
(N'Nguyễn Thị B', 'nguyenb', 'RNuUoxlFpLzHh0ICDUCxGazq95emqPjGUGVskxvxN10=', 2, N'Toán', 'nguyenb@school.edu.vn', '0912345678'),
(N'Trần Văn Thư', 'thuthu', 'RNuUoxlFpLzHh0ICDUCxGazq95emqPjGUGVskxvxN10=', 3, NULL, 'librarian@school.edu.vn', '0987654321'),
(N'Admin Hệ thống', 'admin', 'RNuUoxlFpLzHh0ICDUCxGazq95emqPjGUGVskxvxN10=', 4, NULL, 'admin@school.edu.vn', NULL);

INSERT INTO Categories (CategoryName) VALUES
(N'Văn học'), (N'Toán học'), (N'Vật lý'), (N'Lịch sử'), (N'Ngoại ngữ');

INSERT INTO Publishers (PublisherName, Address, Email, Phone, Description) VALUES
(N'Giáo dục Việt Nam', N'81 Trần Hưng Đạo, Hoàn Kiếm, Hà Nội', 'contact@gdvn.vn', '02438220801', N'Nhà xuất bản giáo dục hàng đầu Việt Nam'),
(N'Kim Đồng', N'55 Quang Trung, Nguyễn Du, Hai Bà Trưng, Hà Nội', 'info@kimdong.vn', '02439434730', N'Nhà xuất bản sách thiếu nhi'),
(N'Trẻ', N'161B Lý Chính Thắng, Phường 7, Quận 3, TP.HCM', 'info@nxbtrẻ.com.vn', '02839316289', N'Nhà xuất bản sách văn học và giáo dục');

INSERT INTO Authors (AuthorName, Biography, Nationality, BirthDate, Description) VALUES
(N'Đoàn Quỳnh', N'Giáo sư Toán học, tác giả nhiều sách giáo khoa', N'Việt Nam', '1940-01-01', N'Chuyên gia về đại số và hình học'),
(N'Lương Duyên Bình', N'Giáo sư Vật lý, tác giả sách giáo khoa Vật lý', N'Việt Nam', '1950-01-01', N'Chuyên gia về vật lý cơ bản'),
(N'Nguyễn Minh Châu', N'Nhà văn nổi tiếng, tác giả nhiều tác phẩm văn học', N'Việt Nam', '1930-10-28', N'Tác giả của nhiều tác phẩm văn học nổi tiếng'),
(N'Nguyễn Du', N'Đại thi hào dân tộc Việt Nam', N'Việt Nam', '1765-01-03', N'Tác giả Truyện Kiều'),
(N'Nam Cao', N'Nhà văn hiện thực phê phán', N'Việt Nam', '1915-10-29', N'Tác giả Chí Phèo, Lão Hạc');

INSERT INTO Books (Title, Author, ISBN, Publisher, PublisherID, PublishYear, CategoryID, TotalCopies) VALUES
(N'Đại số 11', N'Đoàn Quỳnh', '978-604-0-12345-6', N'Giáo dục Việt Nam', 1, 2020, 2, 3),
(N'Vật lý 11', N'Lương Duyên Bình', '978-604-0-65432-1', N'Giáo dục Việt Nam', 1, 2021, 3, 2),
(N'Chiếc thuyền ngoài xa', N'Nguyễn Minh Châu', '978-604-2-98765-4', N'Giáo dục Việt Nam', 1, 2019, 1, 5),
(N'Truyện Kiều', N'Nguyễn Du', '978-604-0-11111-1', N'Kim Đồng', 2, 2020, 1, 10),
(N'Chí Phèo', N'Nam Cao', '978-604-0-22222-2', N'Trẻ', 3, 2021, 1, 8);

-- Tạo quan hệ Many-to-Many giữa Books và Authors
INSERT INTO BookAuthors (BookID, AuthorID) VALUES
(1, 1), -- Đại số 11 - Đoàn Quỳnh
(2, 2), -- Vật lý 11 - Lương Duyên Bình
(3, 3), -- Chiếc thuyền ngoài xa - Nguyễn Minh Châu
(4, 4), -- Truyện Kiều - Nguyễn Du
(5, 5); -- Chí Phèo - Nam Cao

INSERT INTO BookCopies (BookID, CopyCode, Status) VALUES
(1, 'TOAN11-001', 'Available'), (1, 'TOAN11-002', 'Borrowed'), (1, 'TOAN11-003', 'Available'),
(2, 'LY11-001', 'Available'), (2, 'LY11-002', 'Available'),
(3, 'VAN11-001', 'Available'), (3, 'VAN11-002', 'Available');

INSERT INTO BorrowRecords (UserID, CopyID, BorrowDate, DueDate, ReturnDate, IsReturned, FineAmount) VALUES
(1, 2, '2025-10-10', '2025-10-24', NULL, 0, 0),
(2, 4, '2025-10-01', '2025-10-15', '2025-10-14', 1, 0);

INSERT INTO Reservations (UserID, BookID, ReservationDate, ExpiryDate, Status) VALUES
(1, 2, '2025-01-15', '2025-01-22', 'Pending'),
(2, 3, '2025-01-10', '2025-01-17', 'Pending');

INSERT INTO BookSuggestions (UserID, BookTitle, Author, ISBN, Reason, Status, SuggestionDate) VALUES
(1, N'Hóa học 11', N'Nguyễn Xuân Trường', '978-604-0-11111-1', N'Sách cần thiết cho chương trình học', 'Pending', '2025-01-12'),
(2, N'Lịch sử Việt Nam', N'Trần Trọng Kim', '978-604-0-22222-2', N'Sách tham khảo tốt cho giáo viên', 'Pending', '2025-01-08');

INSERT INTO SystemSettings (SettingKey, SettingValue, Description, SettingType, LastModified)
VALUES
(N'DefaultBorrowDays', '14', N'Số ngày mượn mặc định', 'Integer', GETDATE()),
(N'MaxRenewDays', '7', N'Số ngày gia hạn tối đa', 'Integer', GETDATE()),
(N'FinePerDay', '5000', N'Tiền phạt mỗi ngày trễ hạn (VNĐ)', 'Integer', GETDATE()),
(N'MaxBorrowBooks', '5', N'Số sách mượn tối đa mỗi người', 'Integer', GETDATE()),
(N'GracePeriod', '3', N'Số ngày gia hạn không phạt', 'Integer', GETDATE()),
(N'SystemName', N'Thư viện Trường THPT', N'Tên hệ thống', 'String', GETDATE()),
(N'LateNotificationDays', '3', N'Nhắc nhở trước khi hết hạn (ngày)', 'Integer', GETDATE());
GO