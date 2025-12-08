USE dupharma_db;
GO

-- Add medicines to North Branch (BranchId = 2)
INSERT INTO [Batches] ([MedicineId], [BatchNumber], [ExpiryDate], [QuantityOnHand], [PurchasePrice], [SellingPrice], [SupplierId], [ReceivedDate], [BranchId]) VALUES
-- Paracetamol for North Branch
(1, 'B2025001N01', '2027-12-31', 300, 10.00, 15.00, 1, '2025-01-15', 2),
-- Ibuprofen for North Branch
(2, 'B2025002N01', '2027-11-30', 250, 12.00, 18.00, 1, '2025-01-15', 2),
-- Aspirin for North Branch
(5, 'B2025005N01', '2027-08-31', 200, 8.00, 12.00, 3, '2025-01-20', 2);

-- Add medicines to South Branch (BranchId = 3)
INSERT INTO [Batches] ([MedicineId], [BatchNumber], [ExpiryDate], [QuantityOnHand], [PurchasePrice], [SellingPrice], [SupplierId], [ReceivedDate], [BranchId]) VALUES
-- Amoxicillin for South Branch
(3, 'B2025003S01', '2027-10-31', 180, 25.00, 35.00, 2, '2025-01-18', 3),
-- Metformin for South Branch
(6, 'B2025006S01', '2027-07-31', 220, 15.00, 22.00, 2, '2025-01-18', 3),
-- Lisinopril for South Branch
(7, 'B2025007S01', '2027-06-30', 150, 20.00, 30.00, 3, '2025-01-22', 3);

GO

PRINT 'Medicines added to North and South branches successfully!';
PRINT '';
PRINT 'North Branch now has:';
PRINT '  - Paracetamol (Panadol) - 300 units';
PRINT '  - Ibuprofen (Advil) - 250 units';
PRINT '  - Aspirin (Bayer) - 200 units';
PRINT '';
PRINT 'South Branch now has:';
PRINT '  - Amoxicillin (Amoxil) - 180 units';
PRINT '  - Metformin (Glucophage) - 220 units';
PRINT '  - Lisinopril (Prinivil) - 150 units';
