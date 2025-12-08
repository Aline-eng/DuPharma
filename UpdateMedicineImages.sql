USE dupharma_db;
GO

-- Update image URLs for medicines
UPDATE [Medicines] SET [ImageUrl] = '/images/paracetamol-panadol.jpg' WHERE [GenericName] = 'Paracetamol';
UPDATE [Medicines] SET [ImageUrl] = '/images/ibuprofen-advil.jpg' WHERE [GenericName] = 'Ibuprofen';
UPDATE [Medicines] SET [ImageUrl] = '/images/amoxicillin-amoxil.jpg' WHERE [GenericName] = 'Amoxicillin';
UPDATE [Medicines] SET [ImageUrl] = '/images/omeprazole-prilosec.jpg' WHERE [GenericName] = 'Omeprazole';
UPDATE [Medicines] SET [ImageUrl] = '/images/aspirin-bayer.jpg' WHERE [GenericName] = 'Aspirin';
UPDATE [Medicines] SET [ImageUrl] = '/images/metformin-glucophage.jpg' WHERE [GenericName] = 'Metformin';
UPDATE [Medicines] SET [ImageUrl] = '/images/lisinopril-prinivil.jpg' WHERE [GenericName] = 'Lisinopril';
UPDATE [Medicines] SET [ImageUrl] = '/images/atorvastatin-lipitor.jpg' WHERE [GenericName] = 'Atorvastatin';
UPDATE [Medicines] SET [ImageUrl] = '/images/ascorbicacid-cevifer.jpg' WHERE [GenericName] = 'Ascorbic Acid';
GO

PRINT 'Medicine images updated successfully!';
