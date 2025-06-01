create database LibraryManagementDB2
use LibraryManagementDB2
-- Create Tables
CREATE TABLE Members (
    MemberID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) UNIQUE,
    Phone NVARCHAR(15),
    Address NVARCHAR(200),
    MembershipDate DATE DEFAULT GETDATE(),
    Status NVARCHAR(20) DEFAULT 'Active'
);

CREATE TABLE Books (
    BookID INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Author NVARCHAR(100) NOT NULL,
    ISBN NVARCHAR(20) UNIQUE,
    Publisher NVARCHAR(100),
    Category NVARCHAR(50),
    TotalCopies INT DEFAULT 1,
    AvailableCopies INT DEFAULT 1,
    DateAdded DATE DEFAULT GETDATE()
);

CREATE TABLE Transactions (
    TransactionID INT IDENTITY(1,1) PRIMARY KEY,
    MemberID INT FOREIGN KEY REFERENCES Members(MemberID),
    BookID INT FOREIGN KEY REFERENCES Books(BookID),
    IssueDate DATE DEFAULT GETDATE(),
    DueDate DATE,
    ReturnDate DATE NULL,
    Status NVARCHAR(20) DEFAULT 'Issued',
    Fine DECIMAL(10,2) DEFAULT 0
);

CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) UNIQUE NOT NULL,
    Password NVARCHAR(100) NOT NULL,
    Role NVARCHAR(20) DEFAULT 'Admin'
);

-- Create Views
CREATE VIEW BookReports AS
SELECT 
    b.BookID,
    b.Title,
    b.Author,
    b.Category,
    b.TotalCopies,
    b.AvailableCopies,
    COUNT(t.TransactionID) as TimesIssued
FROM Books b
LEFT JOIN Transactions t ON b.BookID = t.BookID
GROUP BY b.BookID, b.Title, b.Author, b.Category, b.TotalCopies, b.AvailableCopies;

CREATE VIEW MemberReports AS
SELECT 
    m.MemberID,
    m.FirstName + ' ' + m.LastName as FullName,
    m.Email,
    m.Phone,
    COUNT(t.TransactionID) as BooksIssued,
    SUM(CASE WHEN t.Status = 'Issued' THEN 1 ELSE 0 END) as CurrentIssues
FROM Members m
LEFT JOIN Transactions t ON m.MemberID = t.MemberID
GROUP BY m.MemberID, m.FirstName, m.LastName, m.Email, m.Phone;

-- Create Triggers
CREATE TRIGGER trg_BookIssue
ON Transactions
AFTER INSERT
AS
BEGIN
    UPDATE Books 
    SET AvailableCopies = AvailableCopies - 1
    WHERE BookID IN (SELECT BookID FROM inserted WHERE Status = 'Issued');
END;

CREATE TRIGGER trg_BookReturn
ON Transactions
AFTER UPDATE
AS
BEGIN
    IF UPDATE(Status)
    BEGIN
        UPDATE Books 
        SET AvailableCopies = AvailableCopies + 1
        WHERE BookID IN (
            SELECT i.BookID 
            FROM inserted i 
            JOIN deleted d ON i.TransactionID = d.TransactionID
            WHERE i.Status = 'Returned' AND d.Status = 'Issued'
        );
    END
END;

-- Insert Sample Data
INSERT INTO Users (Username, Password) VALUES ('admin', 'admin123');

INSERT INTO Members (FirstName, LastName, Email, Phone, Address) VALUES
('John', 'Doe', 'john.doe@email.com', '1234567890', '123 Main St'),
('Jane', 'Smith', 'jane.smith@email.com', '0987654321', '456 Oak Ave'),
('Mike', 'Johnson', 'mike.j@email.com', '5555555555', '789 Pine St');

INSERT INTO Books (Title, Author, ISBN, Publisher, Category, TotalCopies, AvailableCopies) VALUES
('The Great Gatsby', 'F. Scott Fitzgerald', '978-0-7432-7356-5', 'Scribner', 'Fiction', 3, 3),
('To Kill a Mockingbird', 'Harper Lee', '978-0-06-112008-4', 'J.B. Lippincott & Co.', 'Fiction', 2, 2),
('1984', 'George Orwell', '978-0-452-28423-4', 'Secker & Warburg', 'Dystopian', 4, 4),
('Pride and Prejudice', 'Jane Austen', '978-0-14-143951-8', 'Penguin Classics', 'Romance', 2, 2),
('The Catcher in the Rye', 'J.D. Salinger', '978-0-316-76948-0', 'Little, Brown', 'Fiction', 3, 3);



CREATE TABLE Students (
    StudentID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) UNIQUE,
    Phone NVARCHAR(15),
    Address NVARCHAR(200),
    StudentNumber NVARCHAR(20) UNIQUE NOT NULL,
    Department NVARCHAR(50),
    Semester NVARCHAR(10),
    EnrollmentDate DATE DEFAULT GETDATE(),
    Status NVARCHAR(20) DEFAULT 'Active'
);

-- Add Faculty Table
CREATE TABLE Faculty (
    FacultyID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) UNIQUE,
    Phone NVARCHAR(15),
    Address NVARCHAR(200),
    EmployeeID NVARCHAR(20) UNIQUE NOT NULL,
    Department NVARCHAR(50),
    Designation NVARCHAR(50),
    JoiningDate DATE DEFAULT GETDATE(),
    Status NVARCHAR(20) DEFAULT 'Active'
);

-- Update Transactions table to support multiple user types
ALTER TABLE Transactions ADD UserType NVARCHAR(10) DEFAULT 'Member';
ALTER TABLE Transactions ADD UserID INT;

-- Add check constraint for UserType
ALTER TABLE Transactions ADD CONSTRAINT CK_UserType 
CHECK (UserType IN ('Member', 'Student', 'Faculty'));

-- Update existing transactions to have UserType = 'Member' and UserID = MemberID
UPDATE Transactions SET UserType = 'Member', UserID = MemberID WHERE UserType IS NULL;

-- Create updated view for transaction reports
CREATE OR ALTER VIEW TransactionReports AS
SELECT 
    t.TransactionID,
    t.UserType,
    CASE 
        WHEN t.UserType = 'Member' THEN m.FirstName + ' ' + m.LastName
        WHEN t.UserType = 'Student' THEN s.FirstName + ' ' + s.LastName
        WHEN t.UserType = 'Faculty' THEN f.FirstName + ' ' + f.LastName
    END AS UserName,
    CASE 
        WHEN t.UserType = 'Student' THEN s.StudentNumber
        WHEN t.UserType = 'Faculty' THEN f.EmployeeID
        ELSE NULL
    END AS UserNumber,
    b.Title AS BookTitle,
    b.Author,
    t.IssueDate,
    t.DueDate,
    t.ReturnDate,
    t.Status,
    t.Fine
FROM Transactions t
LEFT JOIN Members m ON t.UserType = 'Member' AND t.UserID = m.MemberID
LEFT JOIN Students s ON t.UserType = 'Student' AND t.UserID = s.StudentID
LEFT JOIN Faculty f ON t.UserType = 'Faculty' AND t.UserID = f.FacultyID
JOIN Books b ON t.BookID = b.BookID;