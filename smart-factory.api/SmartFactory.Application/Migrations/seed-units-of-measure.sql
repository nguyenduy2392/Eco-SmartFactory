-- Insert default units of measure
-- Seed data for UnitsOfMeasure table

-- Delete existing data (if any)
DELETE FROM UnitsOfMeasure;

-- Insert common units
INSERT INTO UnitsOfMeasure (Id, Code, Name, Description, DisplayOrder, IsActive, CreatedAt) VALUES
(NEWID(), 'kg', 'Kilogram', 'Đơn vị khối lượng', 1, 1, GETUTCDATE()),
(NEWID(), 'g', 'Gram', 'Đơn vị khối lượng nhỏ', 2, 1, GETUTCDATE()),
(NEWID(), 'ton', 'Tấn', 'Đơn vị khối lượng lớn', 3, 1, GETUTCDATE()),
(NEWID(), 'l', 'Lít', 'Đơn vị thể tích', 4, 1, GETUTCDATE()),
(NEWID(), 'ml', 'Mililít', 'Đơn vị thể tích nhỏ', 5, 1, GETUTCDATE()),
(NEWID(), 'pcs', 'Cái', 'Đơn vị đếm', 6, 1, GETUTCDATE()),
(NEWID(), 'set', 'Bộ', 'Đơn vị tập hợp', 7, 1, GETUTCDATE()),
(NEWID(), 'm', 'Mét', 'Đơn vị độ dài', 8, 1, GETUTCDATE()),
(NEWID(), 'cm', 'Centimet', 'Đơn vị độ dài nhỏ', 9, 1, GETUTCDATE()),
(NEWID(), 'm2', 'Mét vuông', 'Đơn vị diện tích', 10, 1, GETUTCDATE()),
(NEWID(), 'm3', 'Mét khối', 'Đơn vị thể tích', 11, 1, GETUTCDATE()),
(NEWID(), 'box', 'Hộp', 'Đơn vị đóng gói', 12, 1, GETUTCDATE()),
(NEWID(), 'roll', 'Cuộn', 'Đơn vị cuộn', 13, 1, GETUTCDATE()),
(NEWID(), 'sheet', 'Tờ', 'Đơn vị tờ giấy', 14, 1, GETUTCDATE());
