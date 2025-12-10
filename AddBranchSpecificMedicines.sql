USE dupharma_db;
GO

-- Add new medicines for North Branch (5 unique medicines)
INSERT INTO [Medicines] ([GenericName], [BrandName], [Strength], [Form], [Unit], [ReorderLevel], [Description], [ImageUrl], [RequiresPrescription]) VALUES
('Cetirizine', 'Zyrtec', '10mg', 'Tablet', 'Piece', 50, 'Antihistamine for allergy relief', '/images/cetirizine-zyrtec.jpg', 0),
('Vitamin C', 'Redoxon', '1000mg', 'Tablet', 'Piece', 40, 'Vitamin C supplement for immune support', '/images/vitaminc-redoxon.jpg', 0),
('Loratadine', 'Claritin', '10mg', 'Tablet', 'Piece', 45, 'Non-drowsy allergy relief', '/images/loratadine-claritin.jpg', 0),
('Calcium', 'Caltrate', '600mg', 'Tablet', 'Piece', 35, 'Calcium supplement for bone health', '/images/calcium-caltrate.jpg', 0),
('Diclofenac', 'Voltaren', '50mg', 'Tablet', 'Piece', 40, 'Anti-inflammatory pain relief', '/images/diclofenac-voltaren.jpg', 1);

-- Add new medicines for South Branch (5 unique medicines)
INSERT INTO [Medicines] ([GenericName], [BrandName], [Strength], [Form], [Unit], [ReorderLevel], [Description], [ImageUrl], [RequiresPrescription]) VALUES
('Azithromycin', 'Zithromax', '500mg', 'Tablet', 'Piece', 30, 'Antibiotic for bacterial infections', '/images/azithromycin-zithromax.jpg', 1),
('Vitamin D', 'Vigantol', '1000IU', 'Capsule', 'Piece', 35, 'Vitamin D supplement for bone health', '/images/vitamind-vigantol.jpg', 0),
('Multivitamin', 'Centrum', 'Daily', 'Tablet', 'Piece', 40, 'Complete daily multivitamin', '/images/multivitamin-centrum.jpg', 0),
('Omega 3', 'Fish Oil', '1000mg', 'Capsule', 'Piece', 30, 'Omega-3 fatty acids for heart health', '/images/omega3-fishoil.jpg', 0),
('Zinc', 'Zincovit', '50mg', 'Tablet', 'Piece', 25, 'Zinc supplement for immune support', '/images/zinc-zincovit.jpg', 0);

GO

-- Add batches for North Branch medicines (Branch 2)
DECLARE @NorthMedicineStart INT = (SELECT MIN(MedicineId) FROM [Medicines] WHERE GenericName IN ('Cetirizine', 'Vitamin C', 'Loratadine', 'Calcium', 'Diclofenac'));

INSERT INTO [Batches] ([MedicineId], [BatchNumber], [ExpiryDate], [QuantityOnHand], [PurchasePrice], [SellingPrice], [SupplierId], [ReceivedDate], [BranchId]) VALUES
-- Cetirizine
(@NorthMedicineStart, 'NB2025001', '2027-12-31', 250, 8.00, 12.00, 1, '2025-01-15', 2),
-- Vitamin C
(@NorthMedicineStart + 1, 'NB2025002', '2027-11-30', 200, 5.00, 10.00, 2, '2025-01-15', 2),
-- Loratadine
(@NorthMedicineStart + 2, 'NB2025003', '2027-10-31', 220, 9.00, 15.00, 1, '2025-01-15', 2),
-- Calcium
(@NorthMedicineStart + 3, 'NB2025004', '2027-09-30', 180, 7.00, 12.00, 3, '2025-01-15', 2),
-- Diclofenac
(@NorthMedicineStart + 4, 'NB2025005', '2027-08-31', 200, 10.00, 16.00, 2, '2025-01-15', 2);

-- Add batches for South Branch medicines (Branch 3)
DECLARE @SouthMedicineStart INT = (SELECT MIN(MedicineId) FROM [Medicines] WHERE GenericName IN ('Azithromycin', 'Vitamin D', 'Multivitamin', 'Omega 3', 'Zinc'));

INSERT INTO [Batches] ([MedicineId], [BatchNumber], [ExpiryDate], [QuantityOnHand], [PurchasePrice], [SellingPrice], [SupplierId], [ReceivedDate], [BranchId]) VALUES
-- Azithromycin
(@SouthMedicineStart, 'SB2025001', '2027-12-31', 180, 35.00, 50.00, 2, '2025-01-15', 3),
-- Vitamin D
(@SouthMedicineStart + 1, 'SB2025002', '2027-11-30', 200, 8.00, 14.00, 1, '2025-01-15', 3),
-- Multivitamin
(@SouthMedicineStart + 2, 'SB2025003', '2027-10-31', 220, 12.00, 20.00, 3, '2025-01-15', 3),
-- Omega 3
(@SouthMedicineStart + 3, 'SB2025004', '2027-09-30', 170, 15.00, 25.00, 2, '2025-01-15', 3),
-- Zinc
(@SouthMedicineStart + 4, 'SB2025005', '2027-08-31', 150, 6.00, 10.00, 1, '2025-01-15', 3);

GO

PRINT 'Branch-specific medicines added successfully!';
PRINT '';
PRINT 'North Branch (Branch 2) - 5 unique medicines:';
PRINT '  - Cetirizine (Zyrtec) - 250 units';
PRINT '  - Vitamin C (Redoxon) - 200 units';
PRINT '  - Loratadine (Claritin) - 220 units';
PRINT '  - Calcium (Caltrate) - 180 units';
PRINT '  - Diclofenac (Voltaren) - 200 units';
PRINT '';
PRINT 'South Branch (Branch 3) - 5 unique medicines:';
PRINT '  - Azithromycin (Zithromax) - 180 units';
PRINT '  - Vitamin D (Vigantol) - 200 units';
PRINT '  - Multivitamin (Centrum) - 220 units';
PRINT '  - Omega 3 (Fish Oil) - 170 units';
PRINT '  - Zinc (Zincovit) - 150 units';
