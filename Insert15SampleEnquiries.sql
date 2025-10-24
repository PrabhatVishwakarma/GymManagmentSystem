-- ============================================
-- Insert 15 Sample Enquiries for Testing
-- Gym Management System
-- ============================================

USE GymManagementDB;
GO

-- Insert 15 diverse enquiries with varied data
INSERT INTO Enquiries (FirstName, LastName, Email, Phone, DateOfBirth, Gender, Address, City, Occupation, IsWhatsappNumber, CreatedAt, UpdatedAt, IsConverted, Createdby)
VALUES
-- Male Enquiries
('John', 'Smith', 'john.smith@email.com', '9876543210', '1995-03-15', 'Male', '123 Main Street, Andheri', 'Mumbai', 'Software Engineer', 1, GETUTCDATE(), GETUTCDATE(), 0, 'Admin'),
('Michael', 'Johnson', 'michael.j@email.com', '9876543211', '1988-07-22', 'Male', '456 Park Avenue, Connaught Place', 'Delhi', 'Business Owner', 1, GETUTCDATE(), GETUTCDATE(), 0, 'Admin'),
('David', 'Williams', 'david.w@email.com', '9876543212', '1992-11-08', 'Male', '789 Lake Road, Koramangala', 'Bangalore', 'Marketing Manager', 1, GETUTCDATE(), GETUTCDATE(), 0, 'Admin'),
('Robert', 'Brown', 'robert.brown@email.com', '9876543213', '1990-05-30', 'Male', '321 Hill Street, Shivaji Nagar', 'Pune', 'Data Analyst', 1, GETUTCDATE(), GETUTCDATE(), 0, 'Admin'),
('James', 'Davis', 'james.davis@email.com', '9876543214', '1985-09-12', 'Male', '654 Beach Road, Marina', 'Chennai', 'Teacher', 1, GETUTCDATE(), GETUTCDATE(), 0, 'Admin'),
('William', 'Miller', 'william.m@email.com', '9876543215', '1993-02-18', 'Male', '987 Valley View, Banjara Hills', 'Hyderabad', 'Accountant', 1, GETUTCDATE(), GETUTCDATE(), 0, 'Admin'),
('Richard', 'Wilson', 'richard.w@email.com', '9876543216', '1991-06-25', 'Male', '147 Garden Lane, Salt Lake', 'Kolkata', 'Consultant', 0, GETUTCDATE(), GETUTCDATE(), 0, 'Admin'),

-- Female Enquiries
('Sarah', 'Martinez', 'sarah.m@email.com', '9876543217', '1994-12-03', 'Female', '258 Rose Avenue, Navrangpura', 'Ahmedabad', 'Designer', 1, GETUTCDATE(), GETUTCDATE(), 0, 'Admin'),
('Jennifer', 'Garcia', 'jennifer.g@email.com', '9876543218', '1989-04-17', 'Female', '369 Maple Street, C-Scheme', 'Jaipur', 'HR Manager', 1, GETUTCDATE(), GETUTCDATE(), 0, 'Admin'),
('Emily', 'Rodriguez', 'emily.r@email.com', '9876543219', '1996-08-09', 'Female', '741 Oak Drive, Gomti Nagar', 'Lucknow', 'Student', 1, GETUTCDATE(), GETUTCDATE(), 0, 'Admin'),
('Jessica', 'Lee', 'jessica.lee@email.com', '9876543220', '1987-10-21', 'Female', '852 Pine Road, MP Nagar', 'Bhopal', 'Doctor', 1, GETUTCDATE(), GETUTCDATE(), 0, 'Admin'),
('Ashley', 'Walker', 'ashley.w@email.com', '9876543221', '1995-01-14', 'Female', '963 Cedar Street, Sector 17', 'Chandigarh', 'Architect', 1, GETUTCDATE(), GETUTCDATE(), 0, 'Admin'),
('Amanda', 'Hall', 'amanda.h@email.com', '9876543222', '1992-07-28', 'Female', '159 Birch Lane, Vijay Nagar', 'Indore', 'Pharmacist', 0, GETUTCDATE(), GETUTCDATE(), 0, 'Admin'),
('Michelle', 'Allen', 'michelle.a@email.com', '9876543223', '1990-11-05', 'Female', '357 Elm Avenue, Civil Lines', 'Nagpur', 'Journalist', 1, GETUTCDATE(), GETUTCDATE(), 0, 'Admin'),
('Stephanie', 'Young', 'stephanie.y@email.com', '9876543224', '1993-03-19', 'Female', '486 Willow Road, Adajan', 'Surat', 'Entrepreneur', 1, GETUTCDATE(), GETUTCDATE(), 0, 'Admin');

GO

-- Verify the insertion
SELECT COUNT(*) as 'Total Enquiries Inserted' FROM Enquiries;

-- Show the newly inserted enquiries
SELECT 
    EnquiryId,
    CONCAT(FirstName, ' ', LastName) as 'Full Name',
    Email,
    Phone,
    City,
    Gender,
    CONVERT(DATE, DateOfBirth) as 'DOB',
    CASE WHEN IsConverted = 1 THEN 'Converted' ELSE 'Open' END as 'Status'
FROM Enquiries
ORDER BY EnquiryId DESC;

PRINT '';
PRINT '✅ Successfully inserted 15 sample enquiries!';
PRINT '';
PRINT '📊 Data Summary:';
PRINT '   - 7 Male enquiries';
PRINT '   - 8 Female enquiries';
PRINT '   - All marked as Open (not converted)';
PRINT '   - Diverse cities across India';
PRINT '   - Unique phone numbers and emails';
PRINT '';
PRINT '🧪 Now you can test:';
PRINT '   ✅ Search by name (try: "John", "Sarah")';
PRINT '   ✅ Search by email (try: "gmail", ".com")';
PRINT '   ✅ Search by phone (try: "9876")';
PRINT '   ✅ Search by city (try: "Mumbai", "Delhi")';
PRINT '   ✅ Pagination (should see multiple pages)';
PRINT '   ✅ Convert some to members';
PRINT '   ✅ Test Open/Closed tabs';
PRINT '';
PRINT '🚀 Refresh your browser and go to Enquiries page!';
GO

