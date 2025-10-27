USE master;
GO
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'LibraryTHPT')
BEGIN
    ALTER DATABASE LibraryTHPT SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE LibraryTHPT;
END
GO

-- Tạo cơ sở dữ liệu
CREATE DATABASE LibraryTHPT;
GO

USE LibraryTHPT;
GO

-- Bảng vai trò
CREATE TABLE Roles (
    RoleID INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) NOT NULL UNIQUE -- 'Học sinh', 'Giáo viên', 'Thủ thư', 'Admin'
);

-- Bảng người dùng
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL, -- Lưu hash, không lưu mật khẩu thô
    RoleID INT NOT NULL,
    ClassOrDepartment NVARCHAR(50), -- Lớp (nếu là HS) hoặc tổ bộ môn (nếu là GV)
    Email NVARCHAR(100),
    Phone NVARCHAR(15),
    IsActive BIT DEFAULT 1,
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);

-- Bảng thể loại
CREATE TABLE Categories (
    CategoryID INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) NOT NULL UNIQUE
);

-- Bảng sách (thông tin đầu sách)
CREATE TABLE Books (
    BookID INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(255) NOT NULL,
    Author NVARCHAR(255),
    ISBN NVARCHAR(20),
    Publisher NVARCHAR(100),
    PublishYear INT,
    CategoryID INT,
    TotalCopies INT DEFAULT 1, -- Tổng số bản sao (chỉ mang tính tham khảo)
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID)
);

-- Bảng bản sao sách (mỗi dòng = 1 cuốn vật lý)
CREATE TABLE BookCopies (
    CopyID INT PRIMARY KEY IDENTITY(1,1),
    BookID INT NOT NULL,
    CopyCode NVARCHAR(20) NOT NULL UNIQUE, -- Mã định danh cho từng cuốn (VD: TOAN11-001)
    Status NVARCHAR(20) DEFAULT 'Available', -- 'Available', 'Borrowed', 'Lost', 'Damaged'
    FOREIGN KEY (BookID) REFERENCES Books(BookID)
);

-- Bảng mượn sách
CREATE TABLE BorrowRecords (
    BorrowID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    CopyID INT NOT NULL,
    BorrowDate DATE NOT NULL,
    DueDate DATE NOT NULL,
    ReturnDate DATE NULL,
    IsReturned BIT DEFAULT 0,
    FineAmount DECIMAL(10,2) DEFAULT 0, -- Phạt trễ hạn (nếu có)
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (CopyID) REFERENCES BookCopies(CopyID)
);

-- ========== NHẬP DỮ LIỆU ========== --
-- Chèn vai trò
INSERT INTO Roles (RoleName) VALUES 
(N'Học sinh'),
(N'Giáo viên'),
(N'Thủ thư'),
(N'Admin');

-- Người dùng mẫu
INSERT INTO Users (FullName, Username, PasswordHash, RoleID, ClassOrDepartment, Email, Phone) VALUES
(N'Phan Văn A', 'phana', 'hashed123', 1, '11A1', 'phana@school.edu.vn', '0901234567'),
(N'Nguyễn Thị B', 'nguyenb', 'hashed123', 2, N'Toán', 'nguyenb@school.edu.vn', '0912345678'),
(N'Trần Văn Thư', 'thuthu', 'hashed123', 3, NULL, 'librarian@school.edu.vn', '0987654321'),
(N'Admin Hệ thống', 'admin', 'hashed123', 4, NULL, 'admin@school.edu.vn', NULL);

-- Thể loại
INSERT INTO Categories (CategoryName) VALUES
(N'Văn học'),
(N'Toán học'),
(N'Vật lý'),
(N'Lịch sử'),
(N'Ngoại ngữ');

-- Sách mẫu
INSERT INTO Books (Title, Author, ISBN, Publisher, PublishYear, CategoryID, TotalCopies) VALUES
(N'Đại số 11', N'Đoàn Quỳnh', '978-604-0-12345-6', N'Giáo dục Việt Nam', 2020, 2, 3),
(N'Vật lý 11', N'Lương Duyên Bình', '978-604-0-65432-1', N'Giáo dục Việt Nam', 2021, 3, 2),
(N'Chiếc thuyền ngoài xa', N'Nguyễn Minh Châu', '978-604-2-98765-4', N'Giáo dục Việt Nam', 2019, 1, 5);

-- Bản sao sách
INSERT INTO BookCopies (BookID, CopyCode, Status) VALUES
(1, 'TOAN11-001', 'Available'),
(1, 'TOAN11-002', 'Borrowed'),
(1, 'TOAN11-003', 'Available'),
(2, 'LY11-001', 'Available'),
(2, 'LY11-002', 'Available'),
(3, 'VAN11-001', 'Available'),
(3, 'VAN11-002', 'Available');

-- Mượn sách mẫu
INSERT INTO BorrowRecords (UserID, CopyID, BorrowDate, DueDate, ReturnDate, IsReturned, FineAmount) VALUES
(1, 2, '2025-10-10', '2025-10-24', NULL, 0, 0), -- Chưa trả
(2, 4, '2025-10-01', '2025-10-15', '2025-10-14', 1, 0); -- Đã trả