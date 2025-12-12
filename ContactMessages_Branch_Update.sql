-- =============================================
-- ContactMessages Table Branch Updates
-- =============================================
-- This script adds Branch and BranchId columns to the ContactMessages table
-- and establishes the foreign key relationship with the Branches table.

USE dupharma_db;
GO

-- Step 1: Add Branch and IsRead columns if they don't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'ContactMessages' AND COLUMN_NAME = 'Branch')
BEGIN
    ALTER TABLE ContactMessages
    ADD Branch nvarchar(100) NOT NULL DEFAULT 'Main Branch';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'ContactMessages' AND COLUMN_NAME = 'IsRead')
BEGIN
    ALTER TABLE ContactMessages
    ADD IsRead bit NOT NULL DEFAULT 0;
END

-- Step 2: Add BranchId column and foreign key constraint if they don't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'ContactMessages' AND COLUMN_NAME = 'BranchId')
BEGIN
    ALTER TABLE ContactMessages
    ADD BranchId INT NULL;

    ALTER TABLE ContactMessages
    ADD CONSTRAINT FK_ContactMessages_Branches_BranchId
    FOREIGN KEY (BranchId) REFERENCES Branches (BranchId);
END

-- Step 3: Update existing records to have BranchId = 1 (Main Branch) if NULL
UPDATE ContactMessages
SET BranchId = 1
WHERE BranchId IS NULL;

-- Step 4: Verification queries (optional - run these to verify the changes)
-- SELECT Id, FullName, Email, Subject, BranchId, IsReplied FROM ContactMessages;
-- SELECT BranchId, BranchName FROM Branches;

PRINT 'ContactMessages table branch updates completed successfully.';
GO
