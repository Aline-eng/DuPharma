IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ContactMessages' 
      AND COLUMN_NAME = 'Branch'
)
BEGIN
    ALTER TABLE ContactMessages
    ADD Branch NVARCHAR(100) NOT NULL 
        CONSTRAINT DF_ContactMessages_Branch DEFAULT 'Main Branch';
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ContactMessages' 
      AND COLUMN_NAME = 'IsRead'
)
BEGIN
    ALTER TABLE ContactMessages
    ADD IsRead BIT NOT NULL 
        CONSTRAINT DF_ContactMessages_IsRead DEFAULT 0;
END
GO

IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ContactMessages' 
      AND COLUMN_NAME = 'BranchId'
)
BEGIN
    ALTER TABLE ContactMessages
    ADD BranchId INT NULL;
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_ContactMessages_Branches_BranchId'
)
BEGIN
    ALTER TABLE ContactMessages
    ADD CONSTRAINT FK_ContactMessages_Branches_BranchId
    FOREIGN KEY (BranchId)
    REFERENCES Branches (BranchId);
END
GO

UPDATE ContactMessages
SET BranchId = 1
WHERE BranchId IS NULL;
GO
