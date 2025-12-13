USE dupharma_db;
GO

/* ===============================
   1. Add Branch column
   =============================== */
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

/* ===============================
   2. Add IsRead column
   =============================== */
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

/* ===============================
   3. Add BranchId column
   =============================== */
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

/* ===============================
   4. Update existing records
   =============================== */
UPDATE ContactMessages
SET BranchId = 1
WHERE BranchId IS NULL;
GO

/* ===============================
   5. Add Foreign Key constraint
   =============================== */
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

/* ===============================
   6. Optional indexes (recommended)
   =============================== */
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_ContactMessages_BranchId'
)
BEGIN
    CREATE INDEX IX_ContactMessages_BranchId
    ON ContactMessages (BranchId);
END
GO

PRINT 'ContactMessages table updated successfully.';
PRINT '- Branch, IsRead, BranchId added';
PRINT '- Existing records linked to Main Branch';
PRINT '- Foreign key enforced';