-- ═══════════════════════════════════════════════════════════════
--  TEST DATA FOR FOOD ORDERING APP
--  INSERT ONLY — run AFTER db2.sql
-- ═══════════════════════════════════════════════════════════════

USE [FoodOrderingDB]
GO

-- ───────────────────────────────────────────
--  1. USERS  (Admin, Customers, Shippers, RestaurantOwners, Applicants)
-- ───────────────────────────────────────────
SET IDENTITY_INSERT [dbo].[Users] ON;

INSERT INTO [dbo].[Users] (UserId, Email, PasswordHash, FullName, PhoneNumber, AvatarUrl, [Role], [Status], CreatedAt) VALUES
-- Admin
(1,  N'admin@foodorder.com',     N'admin123',    N'System Admin',       N'0900000001', NULL, N'Admin',           N'Active', GETDATE()),

-- Customers
(2,  N'customer1@gmail.com',     N'password123', N'Nguyen Van An',      N'0912345001', NULL, N'Customer',        N'Active', GETDATE()),
(3,  N'customer2@gmail.com',     N'password123', N'Tran Thi Binh',      N'0912345002', NULL, N'Customer',        N'Active', GETDATE()),
(4,  N'customer3@gmail.com',     N'password123', N'Le Hoang Cuong',     N'0912345003', NULL, N'Customer',        N'Active', GETDATE()),

-- Shippers
(5,  N'shipper1@gmail.com',      N'password123', N'Pham Minh Duc',      N'0912345004', NULL, N'Shipper',         N'Active', GETDATE()),
(6,  N'shipper2@gmail.com',      N'password123', N'Vo Thanh Em',        N'0912345005', NULL, N'Shipper',         N'Active', GETDATE()),

-- Restaurant Owners
(7,  N'owner1@gmail.com',        N'password123', N'Hoang Thi Giang',    N'0912345006', NULL, N'RestaurantOwner', N'Active', GETDATE()),
(8,  N'owner2@gmail.com',        N'password123', N'Dang Van Hai',       N'0912345007', NULL, N'RestaurantOwner', N'Active', GETDATE()),
(9,  N'owner3@gmail.com',        N'password123', N'Bui Thi Ich',        N'0912345008', NULL, N'RestaurantOwner', N'Active', GETDATE()),

-- Users with pending applications
(10, N'applicant1@gmail.com',    N'password123', N'Ngo Van Khanh',      N'0912345009', NULL, N'Customer',        N'Active', GETDATE()),
(11, N'applicant2@gmail.com',    N'password123', N'Truong Thi Linh',    N'0912345010', NULL, N'Customer',        N'Active', GETDATE());

SET IDENTITY_INSERT [dbo].[Users] OFF;
GO

-- ───────────────────────────────────────────
--  2. CUSTOMERS
-- ───────────────────────────────────────────
INSERT INTO [dbo].[Customers] (CustomerId, [Address], Latitude, Longitude) VALUES
(2, N'123 Le Loi, Quan 1, TP.HCM',         10.776530, 106.700981),
(3, N'45 Nguyen Hue, Quan 1, TP.HCM',      10.773831, 106.704020),
(4, N'78 Tran Hung Dao, Quan 5, TP.HCM',   10.754020, 106.667260),
(10, N'99 Hai Ba Trung, Quan 1, TP.HCM',   10.780000, 106.700000),
(11, N'15 Vo Van Tan, Quan 3, TP.HCM',     10.775000, 106.690000);
GO

-- ───────────────────────────────────────────
--  3. SHIPPERS
-- ───────────────────────────────────────────
INSERT INTO [dbo].[Shippers] (ShipperId, VehicleType, LicensePlate, IsAvailable, TotalDeliveries) VALUES
(5, N'motorbike', N'59-A1 12345', 1, 42),
(6, N'motorbike', N'59-B2 67890', 1, 18);
GO

-- ───────────────────────────────────────────
--  4. RESTAURANT OWNERS
-- ───────────────────────────────────────────
INSERT INTO [dbo].[RestaurantOwners] (OwnerId) VALUES
(7),
(8),
(9);
GO

-- ───────────────────────────────────────────
--  5. RESTAURANTS
-- ───────────────────────────────────────────
SET IDENTITY_INSERT [dbo].[Restaurants] ON;

INSERT INTO [dbo].[Restaurants] (RestaurantId, OwnerId, [Name], [Address], Latitude, Longitude, [Description], LogoUrl, OpenTime, CloseTime, IsOpen, AverageRating, TotalOrders, CreatedAt) VALUES
(1, 7, N'Pho Ha Noi 36',         N'36 Ly Tu Trong, Quan 1, TP.HCM',      10.775200, 106.699100, N'Pho bo truyen thong Ha Noi',           NULL, '06:00', '22:00', 1, 4.50, 120, GETDATE()),
(2, 8, N'Com Tam Sai Gon',       N'88 Nguyen Thai Binh, Quan 1, TP.HCM', 10.771500, 106.697800, N'Com tam suon nuong dac biet',          NULL, '06:30', '21:00', 1, 4.30, 85,  GETDATE()),
(3, 9, N'Bun Cha Huong Lien',    N'12 Le Thanh Ton, Quan 1, TP.HCM',     10.778400, 106.703200, N'Bun cha kieu Ha Noi',                  NULL, '10:00', '21:30', 1, 4.70, 200, GETDATE());

SET IDENTITY_INSERT [dbo].[Restaurants] OFF;
GO

-- ───────────────────────────────────────────
--  6. FOOD ITEMS
-- ───────────────────────────────────────────
SET IDENTITY_INSERT [dbo].[FoodItems] ON;

INSERT INTO [dbo].[FoodItems] (FoodItemId, RestaurantId, [Name], [Description], Price, ImageUrl, Category, IsAvailable, DailyQuantityLimit, DailyQuantityUsed, QuantityResetDate) VALUES
-- Pho Ha Noi 36
(1,  1, N'Pho bo tai',              N'Pho bo tai truyen thong',         45000,  NULL, N'Pho',       1, 100, 0, NULL),
(2,  1, N'Pho bo chin',             N'Pho bo chin mem',                 45000,  NULL, N'Pho',       1, 100, 0, NULL),
(3,  1, N'Pho bo tai nam gau',      N'Pho dac biet day du topping',     55000,  NULL, N'Pho',       1, 80,  0, NULL),
(4,  1, N'Nuoc chanh tuoi',         N'Chanh tuoi ep tai cho',           15000,  NULL, N'Nuoc uong', 1, NULL, 0, NULL),

-- Com Tam Sai Gon
(5,  2, N'Com tam suon nuong',      N'Suon nuong than hoa',             40000,  NULL, N'Com tam',   1, 120, 0, NULL),
(6,  2, N'Com tam suon bi cha',     N'Suon + bi + cha trung',           50000,  NULL, N'Com tam',   1, 100, 0, NULL),
(7,  2, N'Com tam dac biet',        N'Full topping: suon, bi, cha, trung op la', 60000, NULL, N'Com tam', 1, 60, 0, NULL),
(8,  2, N'Tra da',                  N'Tra da mien phi',                  0,     NULL, N'Nuoc uong', 1, NULL, 0, NULL),

-- Bun Cha Huong Lien
(9,  3, N'Bun cha Ha Noi',          N'Bun cha thit nuong + nem ran',    50000,  NULL, N'Bun cha',   1, 150, 0, NULL),
(10, 3, N'Bun cha Obama',           N'Set combo Obama da an',            75000,  NULL, N'Bun cha',   1, 50,  0, NULL),
(11, 3, N'Nem ran',                  N'Nem ran gion rum (5 cai)',        30000,  NULL, N'Khai vi',   1, 80,  0, NULL),
(12, 3, N'Bia Ha Noi',              N'Bia lon 330ml',                    20000,  NULL, N'Nuoc uong', 1, NULL, 0, NULL);

SET IDENTITY_INSERT [dbo].[FoodItems] OFF;
GO

-- ───────────────────────────────────────────
--  7. ORDERS (various statuses)
-- ───────────────────────────────────────────
SET IDENTITY_INSERT [dbo].[Orders] ON;

INSERT INTO [dbo].[Orders] (OrderId, CustomerId, RestaurantId, ShipperId, TotalAmount, DeliveryAddress, [Status], PaymentMethod, PaymentTransactionId, Note, CreatedAt, UpdatedAt) VALUES
-- Delivered orders
(1, 2, 1, 5, 105000, N'123 Le Loi, Quan 1, TP.HCM',       N'Delivered',       N'VNPay', N'TXN001', N'It hanh',             DATEADD(DAY, -5, GETDATE()), DATEADD(DAY, -5, GETDATE())),
(2, 3, 2, 6, 110000, N'45 Nguyen Hue, Quan 1, TP.HCM',    N'Delivered',       N'VNPay', N'TXN002', NULL,                    DATEADD(DAY, -3, GETDATE()), DATEADD(DAY, -3, GETDATE())),
(3, 4, 3, 5, 155000, N'78 Tran Hung Dao, Quan 5, TP.HCM', N'Delivered',       N'VNPay', N'TXN003', N'Giao nhanh giup',     DATEADD(DAY, -2, GETDATE()), DATEADD(DAY, -2, GETDATE())),

-- In-progress orders
(4, 2, 3, 6,  75000, N'123 Le Loi, Quan 1, TP.HCM',       N'Delivering',      N'VNPay', N'TXN004', NULL,                    DATEADD(HOUR, -1, GETDATE()), DATEADD(HOUR, -1, GETDATE())),
(5, 3, 1, NULL, 55000, N'45 Nguyen Hue, Quan 1, TP.HCM',  N'WaitingFood',     N'VNPay', N'TXN005', N'Them gia',            DATEADD(MINUTE, -30, GETDATE()), DATEADD(MINUTE, -30, GETDATE())),

-- Pending orders
(6, 4, 2, NULL, 60000, N'78 Tran Hung Dao, Quan 5, TP.HCM', N'WaitingShipper', N'VNPay', N'TXN006', NULL,                   GETDATE(), GETDATE()),
(7, 2, 1, NULL, 90000, N'123 Le Loi, Quan 1, TP.HCM',       N'PendingPayment', N'VNPay', NULL,      N'Khong ot',            GETDATE(), GETDATE()),

-- Cancelled order
(8, 3, 3, NULL, 50000, N'45 Nguyen Hue, Quan 1, TP.HCM',    N'Cancelled',      N'VNPay', NULL,      N'Doi y',               DATEADD(DAY, -1, GETDATE()), DATEADD(DAY, -1, GETDATE()));

SET IDENTITY_INSERT [dbo].[Orders] OFF;
GO

-- ───────────────────────────────────────────
--  8. ORDER ITEMS
-- ───────────────────────────────────────────
SET IDENTITY_INSERT [dbo].[OrderItems] ON;

INSERT INTO [dbo].[OrderItems] (OrderItemId, OrderId, FoodItemId, FoodName, FoodPrice, Quantity) VALUES
-- Order 1: Pho bo tai x1 + Pho bo chin x1 + Nuoc chanh x1
(1,  1, 1, N'Pho bo tai',             45000, 1),
(2,  1, 2, N'Pho bo chin',            45000, 1),
(3,  1, 4, N'Nuoc chanh tuoi',        15000, 1),

-- Order 2: Com tam suon bi cha x1 + Com tam dac biet x1
(4,  2, 6, N'Com tam suon bi cha',    50000, 1),
(5,  2, 7, N'Com tam dac biet',       60000, 1),

-- Order 3: Bun cha Obama x1 + Nem ran x1 + Bia x2
(6,  3, 10, N'Bun cha Obama',         75000, 1),
(7,  3, 11, N'Nem ran',               30000, 1),
(8,  3, 12, N'Bia Ha Noi',            20000, 2),

-- Order 4: Bun cha Obama x1
(9,  4, 10, N'Bun cha Obama',         75000, 1),

-- Order 5: Pho bo tai nam gau x1
(10, 5, 3,  N'Pho bo tai nam gau',    55000, 1),

-- Order 6: Com tam dac biet x1
(11, 6, 7,  N'Com tam dac biet',      60000, 1),

-- Order 7: Pho bo tai x2
(12, 7, 1,  N'Pho bo tai',            45000, 2),

-- Order 8 (cancelled): Bun cha Ha Noi x1
(13, 8, 9,  N'Bun cha Ha Noi',        50000, 1);

SET IDENTITY_INSERT [dbo].[OrderItems] OFF;
GO

-- ───────────────────────────────────────────
--  9. SHIPPER APPLICATIONS (Pending — for admin to test approve/reject)
-- ───────────────────────────────────────────
SET IDENTITY_INSERT [dbo].[ShipperApplications] ON;

INSERT INTO [dbo].[ShipperApplications] (ApplicationId, UserId, VehicleType, LicensePlate, IdentityCard, IdentityCardFrontUrl, IdentityCardBackUrl, [Status], AdminNote, SubmittedAt, ReviewedAt) VALUES
(1, 10, N'motorbike', N'59-C3 11111', N'012345678901', NULL, NULL, N'Pending',  NULL, DATEADD(DAY, -1, GETDATE()), NULL),
(2, 11, N'bicycle',   N'N/A',         N'098765432109', NULL, NULL, N'Pending',  NULL, GETDATE(), NULL);

SET IDENTITY_INSERT [dbo].[ShipperApplications] OFF;
GO

-- ───────────────────────────────────────────
--  10. RESTAURANT APPLICATIONS (Pending — for admin to test approve/reject)
-- ───────────────────────────────────────────
SET IDENTITY_INSERT [dbo].[RestaurantApplications] ON;

INSERT INTO [dbo].[RestaurantApplications] (ApplicationId, UserId, RestaurantName, [Address], Latitude, Longitude, [Description], LogoUrl, OpenTime, CloseTime, [Status], AdminNote, SubmittedAt, ReviewedAt) VALUES
(1, 10, N'Banh Mi Huynh Hoa',   N'26 Le Thi Rieng, Quan 1, TP.HCM', 10.770200, 106.690100, N'Banh mi noi tieng nhat Sai Gon',   NULL, '15:00', '23:00', N'Pending', NULL, DATEADD(DAY, -2, GETDATE()), NULL),
(2, 11, N'Hu Tieu Nam Vang',    N'50 Ky Con, Quan 1, TP.HCM',       10.769800, 106.695400, N'Hu tieu kieu Nam Vang dac biet',    NULL, '06:00', '14:00', N'Pending', NULL, GETDATE(), NULL);

SET IDENTITY_INSERT [dbo].[RestaurantApplications] OFF;
GO


PRINT N''
PRINT N'============================================='
PRINT N'  TEST DATA INSERTED SUCCESSFULLY!'
PRINT N'============================================='
PRINT N''
PRINT N'  LOGIN ACCOUNTS:'
PRINT N'  ─────────────────────────────────────────'
PRINT N'  Admin:        admin@foodorder.com    / admin123'
PRINT N'  Customer 1:   customer1@gmail.com    / password123'
PRINT N'  Customer 2:   customer2@gmail.com    / password123'
PRINT N'  Customer 3:   customer3@gmail.com    / password123'
PRINT N'  Shipper 1:    shipper1@gmail.com     / password123'
PRINT N'  Shipper 2:    shipper2@gmail.com     / password123'
PRINT N'  Owner 1:      owner1@gmail.com       / password123'
PRINT N'  Owner 2:      owner2@gmail.com       / password123'
PRINT N'  Owner 3:      owner3@gmail.com       / password123'
PRINT N''
PRINT N'  PENDING APPLICATIONS: 2 shipper + 2 restaurant'
PRINT N'  ORDERS: 3 delivered, 2 in-progress, 2 pending, 1 cancelled'
PRINT N'============================================='
GO
