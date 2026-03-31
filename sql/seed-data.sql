-- Generated 2026-03-29 17:10:13 from [.\SQLEXPRESS].[AbuAmenPharmaDb]
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRAN;

-- [Units] rows: 2
SET IDENTITY_INSERT [dbo].[Units] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[Units] WHERE [Id] = 1)
    INSERT INTO [dbo].[Units] ([Id], [NameAr], [NameEn], [IsActive]) VALUES (1, N'شريط', NULL, 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[Units] WHERE [Id] = 2)
    INSERT INTO [dbo].[Units] ([Id], [NameAr], [NameEn], [IsActive]) VALUES (2, N'زجاجة', NULL, 1);
SET IDENTITY_INSERT [dbo].[Units] OFF;

-- [Manufacturers] rows: 2
SET IDENTITY_INSERT [dbo].[Manufacturers] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[Manufacturers] WHERE [Id] = 1)
    INSERT INTO [dbo].[Manufacturers] ([Id], [NameAr], [Country], [IsActive]) VALUES (1, N'contraSub', NULL, 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[Manufacturers] WHERE [Id] = 2)
    INSERT INTO [dbo].[Manufacturers] ([Id], [NameAr], [Country], [IsActive]) VALUES (2, N'المعز', NULL, 1);
SET IDENTITY_INSERT [dbo].[Manufacturers] OFF;

-- [Salesmen] rows: 1
SET IDENTITY_INSERT [dbo].[Salesmen] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[Salesmen] WHERE [Id] = 1)
    INSERT INTO [dbo].[Salesmen] ([Id], [NameAr], [Phone], [IsActive]) VALUES (1, N'احمد', NULL, 1);
SET IDENTITY_INSERT [dbo].[Salesmen] OFF;

-- [Customers] rows: 4
SET IDENTITY_INSERT [dbo].[Customers] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[Customers] WHERE [Id] = 1)
    INSERT INTO [dbo].[Customers] ([Id], [Name], [Phone], [Balance], [SalesmanId], [IsActive]) VALUES (1, N'الحارث', NULL, 0.00, NULL, 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[Customers] WHERE [Id] = 2)
    INSERT INTO [dbo].[Customers] ([Id], [Name], [Phone], [Balance], [SalesmanId], [IsActive]) VALUES (2, N'صيدلية الشفاء', NULL, 0.00, NULL, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[Customers] WHERE [Id] = 3)
    INSERT INTO [dbo].[Customers] ([Id], [Name], [Phone], [Balance], [SalesmanId], [IsActive]) VALUES (3, N'صيدلية الشفاء', NULL, 0.00, NULL, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[Customers] WHERE [Id] = 4)
    INSERT INTO [dbo].[Customers] ([Id], [Name], [Phone], [Balance], [SalesmanId], [IsActive]) VALUES (4, N'حسن', NULL, 0.00, NULL, 1);
SET IDENTITY_INSERT [dbo].[Customers] OFF;

-- [Suppliers] rows: 1
SET IDENTITY_INSERT [dbo].[Suppliers] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[Suppliers] WHERE [Id] = 1)
    INSERT INTO [dbo].[Suppliers] ([Id], [Name], [Phone], [Balance], [IsActive]) VALUES (1, N'LongShien', NULL, 0.00, 1);
SET IDENTITY_INSERT [dbo].[Suppliers] OFF;

-- [Items] rows: 2
SET IDENTITY_INSERT [dbo].[Items] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[Items] WHERE [Id] = 1)
    INSERT INTO [dbo].[Items] ([Id], [NameAr], [GenericName], [BarCode], [ManufacturerId], [UnitId], [DefaultPurchasePrice], [DefaultSellPrice], [ReorderLevel], [IsActive]) VALUES (1, N'بارستمول', N'paracatamol', NULL, 1, 1, 100.00, 250.00, 0, 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[Items] WHERE [Id] = 2)
    INSERT INTO [dbo].[Items] ([Id], [NameAr], [GenericName], [BarCode], [ManufacturerId], [UnitId], [DefaultPurchasePrice], [DefaultSellPrice], [ReorderLevel], [IsActive]) VALUES (2, N'فلاجين', N'فلاجين', NULL, 2, 2, 200.00, 300.00, 0, 1);
SET IDENTITY_INSERT [dbo].[Items] OFF;

-- [ItemBatches] rows: 3
SET IDENTITY_INSERT [dbo].[ItemBatches] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[ItemBatches] WHERE [Id] = 1)
    INSERT INTO [dbo].[ItemBatches] ([Id], [ItemId], [BatchNo], [ExpiryDate], [PurchasePrice], [SellPrice], [CreatedAt], [IsActive]) VALUES (1, 1, N'1', '2026-03-31 00:00:00.0000000', 200.00, 250.00, '2026-03-29 14:40:11.4466827', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[ItemBatches] WHERE [Id] = 2)
    INSERT INTO [dbo].[ItemBatches] ([Id], [ItemId], [BatchNo], [ExpiryDate], [PurchasePrice], [SellPrice], [CreatedAt], [IsActive]) VALUES (2, 1, N'2', '2026-04-08 00:00:00.0000000', 100.00, 250.00, '2026-03-29 15:05:27.7153350', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[ItemBatches] WHERE [Id] = 3)
    INSERT INTO [dbo].[ItemBatches] ([Id], [ItemId], [BatchNo], [ExpiryDate], [PurchasePrice], [SellPrice], [CreatedAt], [IsActive]) VALUES (3, 1, N'3', '2026-04-27 00:00:00.0000000', 500.00, 250.00, '2026-03-29 16:37:03.7501616', 1);
SET IDENTITY_INSERT [dbo].[ItemBatches] OFF;

-- [Purchases] rows: 3
SET IDENTITY_INSERT [dbo].[Purchases] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[Purchases] WHERE [Id] = 1)
    INSERT INTO [dbo].[Purchases] ([Id], [PurchaseDate], [SupplierId], [InvoiceNo], [SubTotal], [Discount], [NetTotal], [IsPosted], [Notes], [CreatedAt]) VALUES (1, '2026-03-29 14:39:25.0010000', 1, N'123445', 400.00, 0.00, 400.00, 1, NULL, '2026-03-29 14:40:11.4300411');
IF NOT EXISTS (SELECT 1 FROM [dbo].[Purchases] WHERE [Id] = 2)
    INSERT INTO [dbo].[Purchases] ([Id], [PurchaseDate], [SupplierId], [InvoiceNo], [SubTotal], [Discount], [NetTotal], [IsPosted], [Notes], [CreatedAt]) VALUES (2, '2026-03-29 15:04:49.5330000', 1, N'2345', 2000.00, 0.00, 2000.00, 1, NULL, '2026-03-29 15:05:27.7007736');
IF NOT EXISTS (SELECT 1 FROM [dbo].[Purchases] WHERE [Id] = 3)
    INSERT INTO [dbo].[Purchases] ([Id], [PurchaseDate], [SupplierId], [InvoiceNo], [SubTotal], [Discount], [NetTotal], [IsPosted], [Notes], [CreatedAt]) VALUES (3, '2026-03-29 16:36:43.4480000', 1, N'', 50000.00, 0.00, 50000.00, 1, NULL, '2026-03-29 16:37:03.7381349');
SET IDENTITY_INSERT [dbo].[Purchases] OFF;

-- [PurchaseLines] rows: 3
SET IDENTITY_INSERT [dbo].[PurchaseLines] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[PurchaseLines] WHERE [Id] = 1)
    INSERT INTO [dbo].[PurchaseLines] ([Id], [PurchaseId], [ItemId], [ExpiryDate], [Qty], [UnitCost], [LineTotal], [BatchId]) VALUES (1, 1, 1, '2026-03-31 00:00:00.0000000', 2.00, 200.00, 400.00, 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[PurchaseLines] WHERE [Id] = 2)
    INSERT INTO [dbo].[PurchaseLines] ([Id], [PurchaseId], [ItemId], [ExpiryDate], [Qty], [UnitCost], [LineTotal], [BatchId]) VALUES (2, 2, 1, '2026-04-08 00:00:00.0000000', 20.00, 100.00, 2000.00, 2);
IF NOT EXISTS (SELECT 1 FROM [dbo].[PurchaseLines] WHERE [Id] = 3)
    INSERT INTO [dbo].[PurchaseLines] ([Id], [PurchaseId], [ItemId], [ExpiryDate], [Qty], [UnitCost], [LineTotal], [BatchId]) VALUES (3, 3, 1, '2026-04-27 00:00:00.0000000', 100.00, 500.00, 50000.00, 3);
SET IDENTITY_INSERT [dbo].[PurchaseLines] OFF;

-- [PurchaseReturns] rows: 0

-- [PurchaseReturnLines] rows: 0

-- [Sales] rows: 8
SET IDENTITY_INSERT [dbo].[Sales] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[Sales] WHERE [Id] = 1)
    INSERT INTO [dbo].[Sales] ([Id], [SaleDate], [CustomerId], [SalesmanId], [PaymentMode], [SubTotal], [Discount], [NetTotal], [PaidAmount], [RemainingAmount], [IsPosted], [Notes]) VALUES (1, '2026-03-29 14:45:11.4500000', 1, 1, 1, 0.00, 50.00, 0.00, 450.00, 0.00, 1, NULL);
IF NOT EXISTS (SELECT 1 FROM [dbo].[Sales] WHERE [Id] = 2)
    INSERT INTO [dbo].[Sales] ([Id], [SaleDate], [CustomerId], [SalesmanId], [PaymentMode], [SubTotal], [Discount], [NetTotal], [PaidAmount], [RemainingAmount], [IsPosted], [Notes]) VALUES (2, '2026-03-29 14:54:15.8160000', 1, 1, 2, 500.00, 0.00, 500.00, 500.00, 0.00, 1, NULL);
IF NOT EXISTS (SELECT 1 FROM [dbo].[Sales] WHERE [Id] = 3)
    INSERT INTO [dbo].[Sales] ([Id], [SaleDate], [CustomerId], [SalesmanId], [PaymentMode], [SubTotal], [Discount], [NetTotal], [PaidAmount], [RemainingAmount], [IsPosted], [Notes]) VALUES (3, '2026-03-29 15:50:10.2010000', 1, 1, 2, 1000.00, 0.00, 1000.00, 1000.00, 0.00, 1, NULL);
IF NOT EXISTS (SELECT 1 FROM [dbo].[Sales] WHERE [Id] = 4)
    INSERT INTO [dbo].[Sales] ([Id], [SaleDate], [CustomerId], [SalesmanId], [PaymentMode], [SubTotal], [Discount], [NetTotal], [PaidAmount], [RemainingAmount], [IsPosted], [Notes]) VALUES (4, '2026-03-29 16:07:07.5760000', 1, 1, 2, 2500.00, 0.00, 2500.00, 2500.00, 0.00, 1, NULL);
IF NOT EXISTS (SELECT 1 FROM [dbo].[Sales] WHERE [Id] = 5)
    INSERT INTO [dbo].[Sales] ([Id], [SaleDate], [CustomerId], [SalesmanId], [PaymentMode], [SubTotal], [Discount], [NetTotal], [PaidAmount], [RemainingAmount], [IsPosted], [Notes]) VALUES (5, '2026-03-29 16:26:48.4710000', 4, 1, 1, 750.00, 0.00, 750.00, 750.00, 0.00, 1, NULL);
IF NOT EXISTS (SELECT 1 FROM [dbo].[Sales] WHERE [Id] = 6)
    INSERT INTO [dbo].[Sales] ([Id], [SaleDate], [CustomerId], [SalesmanId], [PaymentMode], [SubTotal], [Discount], [NetTotal], [PaidAmount], [RemainingAmount], [IsPosted], [Notes]) VALUES (6, '2026-03-29 16:28:01.8220000', 4, 1, 2, 750.00, 0.00, 750.00, 750.00, 0.00, 1, NULL);
IF NOT EXISTS (SELECT 1 FROM [dbo].[Sales] WHERE [Id] = 9)
    INSERT INTO [dbo].[Sales] ([Id], [SaleDate], [CustomerId], [SalesmanId], [PaymentMode], [SubTotal], [Discount], [NetTotal], [PaidAmount], [RemainingAmount], [IsPosted], [Notes]) VALUES (9, '2026-03-29 16:37:11.0200000', 4, 1, 2, 500.00, 0.00, 500.00, 500.00, 0.00, 1, N'تم استخدام رصيد عميل بمبلغ 500.00');
IF NOT EXISTS (SELECT 1 FROM [dbo].[Sales] WHERE [Id] = 10)
    INSERT INTO [dbo].[Sales] ([Id], [SaleDate], [CustomerId], [SalesmanId], [PaymentMode], [SubTotal], [Discount], [NetTotal], [PaidAmount], [RemainingAmount], [IsPosted], [Notes]) VALUES (10, '2026-03-29 16:37:59.3460000', 4, 1, 2, 2500.00, 0.00, 2500.00, 2500.00, 0.00, 1, NULL);
SET IDENTITY_INSERT [dbo].[Sales] OFF;

-- [SaleLines] rows: 8
SET IDENTITY_INSERT [dbo].[SaleLines] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[SaleLines] WHERE [Id] = 1)
    INSERT INTO [dbo].[SaleLines] ([Id], [SaleId], [ItemId], [Qty], [UnitPrice], [LineTotal]) VALUES (1, 1, 1, 2.00, 250.00, 500.00);
IF NOT EXISTS (SELECT 1 FROM [dbo].[SaleLines] WHERE [Id] = 2)
    INSERT INTO [dbo].[SaleLines] ([Id], [SaleId], [ItemId], [Qty], [UnitPrice], [LineTotal]) VALUES (2, 2, 1, 2.00, 250.00, 500.00);
IF NOT EXISTS (SELECT 1 FROM [dbo].[SaleLines] WHERE [Id] = 3)
    INSERT INTO [dbo].[SaleLines] ([Id], [SaleId], [ItemId], [Qty], [UnitPrice], [LineTotal]) VALUES (3, 3, 1, 4.00, 250.00, 1000.00);
IF NOT EXISTS (SELECT 1 FROM [dbo].[SaleLines] WHERE [Id] = 4)
    INSERT INTO [dbo].[SaleLines] ([Id], [SaleId], [ItemId], [Qty], [UnitPrice], [LineTotal]) VALUES (4, 4, 1, 10.00, 250.00, 2500.00);
IF NOT EXISTS (SELECT 1 FROM [dbo].[SaleLines] WHERE [Id] = 5)
    INSERT INTO [dbo].[SaleLines] ([Id], [SaleId], [ItemId], [Qty], [UnitPrice], [LineTotal]) VALUES (5, 5, 1, 3.00, 250.00, 750.00);
IF NOT EXISTS (SELECT 1 FROM [dbo].[SaleLines] WHERE [Id] = 6)
    INSERT INTO [dbo].[SaleLines] ([Id], [SaleId], [ItemId], [Qty], [UnitPrice], [LineTotal]) VALUES (6, 6, 1, 3.00, 250.00, 750.00);
IF NOT EXISTS (SELECT 1 FROM [dbo].[SaleLines] WHERE [Id] = 9)
    INSERT INTO [dbo].[SaleLines] ([Id], [SaleId], [ItemId], [Qty], [UnitPrice], [LineTotal]) VALUES (9, 9, 1, 2.00, 250.00, 500.00);
IF NOT EXISTS (SELECT 1 FROM [dbo].[SaleLines] WHERE [Id] = 10)
    INSERT INTO [dbo].[SaleLines] ([Id], [SaleId], [ItemId], [Qty], [UnitPrice], [LineTotal]) VALUES (10, 10, 1, 10.00, 250.00, 2500.00);
SET IDENTITY_INSERT [dbo].[SaleLines] OFF;

-- [SaleAllocations] rows: 8
SET IDENTITY_INSERT [dbo].[SaleAllocations] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[SaleAllocations] WHERE [Id] = 1)
    INSERT INTO [dbo].[SaleAllocations] ([Id], [SaleLineId], [BatchId], [Qty]) VALUES (1, 1, 1, 2.00);
IF NOT EXISTS (SELECT 1 FROM [dbo].[SaleAllocations] WHERE [Id] = 2)
    INSERT INTO [dbo].[SaleAllocations] ([Id], [SaleLineId], [BatchId], [Qty]) VALUES (2, 2, 1, 2.00);
IF NOT EXISTS (SELECT 1 FROM [dbo].[SaleAllocations] WHERE [Id] = 3)
    INSERT INTO [dbo].[SaleAllocations] ([Id], [SaleLineId], [BatchId], [Qty]) VALUES (3, 3, 2, 4.00);
IF NOT EXISTS (SELECT 1 FROM [dbo].[SaleAllocations] WHERE [Id] = 4)
    INSERT INTO [dbo].[SaleAllocations] ([Id], [SaleLineId], [BatchId], [Qty]) VALUES (4, 4, 2, 10.00);
IF NOT EXISTS (SELECT 1 FROM [dbo].[SaleAllocations] WHERE [Id] = 5)
    INSERT INTO [dbo].[SaleAllocations] ([Id], [SaleLineId], [BatchId], [Qty]) VALUES (5, 5, 2, 3.00);
IF NOT EXISTS (SELECT 1 FROM [dbo].[SaleAllocations] WHERE [Id] = 6)
    INSERT INTO [dbo].[SaleAllocations] ([Id], [SaleLineId], [BatchId], [Qty]) VALUES (6, 6, 2, 3.00);
IF NOT EXISTS (SELECT 1 FROM [dbo].[SaleAllocations] WHERE [Id] = 7)
    INSERT INTO [dbo].[SaleAllocations] ([Id], [SaleLineId], [BatchId], [Qty]) VALUES (7, 9, 3, 2.00);
IF NOT EXISTS (SELECT 1 FROM [dbo].[SaleAllocations] WHERE [Id] = 8)
    INSERT INTO [dbo].[SaleAllocations] ([Id], [SaleLineId], [BatchId], [Qty]) VALUES (8, 10, 3, 10.00);
SET IDENTITY_INSERT [dbo].[SaleAllocations] OFF;

-- [CustomerReceipts] rows: 12
SET IDENTITY_INSERT [dbo].[CustomerReceipts] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerReceipts] WHERE [Id] = 1)
    INSERT INTO [dbo].[CustomerReceipts] ([Id], [Date], [CustomerId], [Amount], [Notes], [IsActive], [UnallocatedAmount]) VALUES (1, '2026-03-29 14:52:41.6730000', 1, 20000.00, NULL, 0, 20000.00);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerReceipts] WHERE [Id] = 2)
    INSERT INTO [dbo].[CustomerReceipts] ([Id], [Date], [CustomerId], [Amount], [Notes], [IsActive], [UnallocatedAmount]) VALUES (2, '2026-03-29 15:08:32.2820000', 1, 500.00, NULL, 0, 0.00);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerReceipts] WHERE [Id] = 3)
    INSERT INTO [dbo].[CustomerReceipts] ([Id], [Date], [CustomerId], [Amount], [Notes], [IsActive], [UnallocatedAmount]) VALUES (3, '2026-03-29 15:40:11.2180000', 1, 500.00, NULL, 1, 0.00);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerReceipts] WHERE [Id] = 4)
    INSERT INTO [dbo].[CustomerReceipts] ([Id], [Date], [CustomerId], [Amount], [Notes], [IsActive], [UnallocatedAmount]) VALUES (4, '2026-03-29 15:41:03.4530000', 1, 500.00, NULL, 0, 500.00);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerReceipts] WHERE [Id] = 5)
    INSERT INTO [dbo].[CustomerReceipts] ([Id], [Date], [CustomerId], [Amount], [Notes], [IsActive], [UnallocatedAmount]) VALUES (5, '2026-03-29 15:52:01.8210000', 1, 750.00, NULL, 1, 0.00);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerReceipts] WHERE [Id] = 6)
    INSERT INTO [dbo].[CustomerReceipts] ([Id], [Date], [CustomerId], [Amount], [Notes], [IsActive], [UnallocatedAmount]) VALUES (6, '2026-03-29 15:52:15.9800000', 1, 250.00, NULL, 0, 0.00);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerReceipts] WHERE [Id] = 7)
    INSERT INTO [dbo].[CustomerReceipts] ([Id], [Date], [CustomerId], [Amount], [Notes], [IsActive], [UnallocatedAmount]) VALUES (7, '2026-03-29 15:56:08.5980000', 1, 250.00, NULL, 1, 0.00);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerReceipts] WHERE [Id] = 8)
    INSERT INTO [dbo].[CustomerReceipts] ([Id], [Date], [CustomerId], [Amount], [Notes], [IsActive], [UnallocatedAmount]) VALUES (8, '2026-03-29 16:07:54.3940000', 1, 2000.00, NULL, 1, 0.00);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerReceipts] WHERE [Id] = 9)
    INSERT INTO [dbo].[CustomerReceipts] ([Id], [Date], [CustomerId], [Amount], [Notes], [IsActive], [UnallocatedAmount]) VALUES (9, '2026-03-29 16:08:21.0720000', 1, 500.00, NULL, 1, 0.00);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerReceipts] WHERE [Id] = 10)
    INSERT INTO [dbo].[CustomerReceipts] ([Id], [Date], [CustomerId], [Amount], [Notes], [IsActive], [UnallocatedAmount]) VALUES (10, '2026-03-29 16:26:34.0900000', 4, 500.00, NULL, 0, 500.00);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerReceipts] WHERE [Id] = 11)
    INSERT INTO [dbo].[CustomerReceipts] ([Id], [Date], [CustomerId], [Amount], [Notes], [IsActive], [UnallocatedAmount]) VALUES (11, '2026-03-29 16:34:03.6490000', 4, 750.00, NULL, 1, 0.00);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerReceipts] WHERE [Id] = 12)
    INSERT INTO [dbo].[CustomerReceipts] ([Id], [Date], [CustomerId], [Amount], [Notes], [IsActive], [UnallocatedAmount]) VALUES (12, '2026-03-29 16:38:52.0790000', 4, 2500.00, NULL, 1, 0.00);
SET IDENTITY_INSERT [dbo].[CustomerReceipts] OFF;

-- [CustomerReceiptAllocations] rows: 9
SET IDENTITY_INSERT [dbo].[CustomerReceiptAllocations] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerReceiptAllocations] WHERE [Id] = 1)
    INSERT INTO [dbo].[CustomerReceiptAllocations] ([Id], [ReceiptId], [SaleId], [Amount], [IsActive]) VALUES (1, 2, 2, 500.00, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerReceiptAllocations] WHERE [Id] = 2)
    INSERT INTO [dbo].[CustomerReceiptAllocations] ([Id], [ReceiptId], [SaleId], [Amount], [IsActive]) VALUES (2, 3, 2, 500.00, 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerReceiptAllocations] WHERE [Id] = 3)
    INSERT INTO [dbo].[CustomerReceiptAllocations] ([Id], [ReceiptId], [SaleId], [Amount], [IsActive]) VALUES (3, 5, 3, 750.00, 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerReceiptAllocations] WHERE [Id] = 4)
    INSERT INTO [dbo].[CustomerReceiptAllocations] ([Id], [ReceiptId], [SaleId], [Amount], [IsActive]) VALUES (4, 6, 3, 250.00, 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerReceiptAllocations] WHERE [Id] = 5)
    INSERT INTO [dbo].[CustomerReceiptAllocations] ([Id], [ReceiptId], [SaleId], [Amount], [IsActive]) VALUES (5, 7, 3, 250.00, 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerReceiptAllocations] WHERE [Id] = 6)
    INSERT INTO [dbo].[CustomerReceiptAllocations] ([Id], [ReceiptId], [SaleId], [Amount], [IsActive]) VALUES (6, 8, 4, 2000.00, 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerReceiptAllocations] WHERE [Id] = 7)
    INSERT INTO [dbo].[CustomerReceiptAllocations] ([Id], [ReceiptId], [SaleId], [Amount], [IsActive]) VALUES (7, 9, 4, 500.00, 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerReceiptAllocations] WHERE [Id] = 8)
    INSERT INTO [dbo].[CustomerReceiptAllocations] ([Id], [ReceiptId], [SaleId], [Amount], [IsActive]) VALUES (8, 11, 6, 750.00, 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerReceiptAllocations] WHERE [Id] = 9)
    INSERT INTO [dbo].[CustomerReceiptAllocations] ([Id], [ReceiptId], [SaleId], [Amount], [IsActive]) VALUES (9, 12, 10, 2500.00, 1);
SET IDENTITY_INSERT [dbo].[CustomerReceiptAllocations] OFF;

-- [SaleReturns] rows: 1
SET IDENTITY_INSERT [dbo].[SaleReturns] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[SaleReturns] WHERE [Id] = 1)
    INSERT INTO [dbo].[SaleReturns] ([Id], [ReturnDate], [SaleId], [CustomerId], [SubTotal], [Discount], [NetTotal], [Notes], [IsActive]) VALUES (1, '2026-03-29 14:46:25.2360000', 1, 1, 500.00, 0.00, 500.00, NULL, 1);
SET IDENTITY_INSERT [dbo].[SaleReturns] OFF;

-- [SaleReturnLines] rows: 1
SET IDENTITY_INSERT [dbo].[SaleReturnLines] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[SaleReturnLines] WHERE [Id] = 1)
    INSERT INTO [dbo].[SaleReturnLines] ([Id], [SaleReturnId], [SaleLineId], [ItemId], [Qty], [UnitPrice], [LineTotal]) VALUES (1, 1, 1, 1, 2.00, 250.00, 500.00);
SET IDENTITY_INSERT [dbo].[SaleReturnLines] OFF;

-- [SaleReturnAllocations] rows: 1
SET IDENTITY_INSERT [dbo].[SaleReturnAllocations] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[SaleReturnAllocations] WHERE [Id] = 1)
    INSERT INTO [dbo].[SaleReturnAllocations] ([Id], [SaleReturnLineId], [BatchId], [Qty], [IsActive]) VALUES (1, 1, 1, 2.00, 1);
SET IDENTITY_INSERT [dbo].[SaleReturnAllocations] OFF;

-- [CustomerLedgers] rows: 27
SET IDENTITY_INSERT [dbo].[CustomerLedgers] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerLedgers] WHERE [Id] = 1)
    INSERT INTO [dbo].[CustomerLedgers] ([Id], [Date], [CustomerId], [Type], [RefId], [Debit], [Credit], [Notes], [IsActive]) VALUES (1, '2026-03-29 14:45:11.4500000', 1, 1, 1, 450.00, 0.00, N'فاتورة بيع رقم 1', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerLedgers] WHERE [Id] = 2)
    INSERT INTO [dbo].[CustomerLedgers] ([Id], [Date], [CustomerId], [Type], [RefId], [Debit], [Credit], [Notes], [IsActive]) VALUES (2, '2026-03-29 14:45:11.4500000', 1, 2, 1, 0.00, 450.00, N'تحصيل داخل فاتورة بيع رقم 1', 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerLedgers] WHERE [Id] = 3)
    INSERT INTO [dbo].[CustomerLedgers] ([Id], [Date], [CustomerId], [Type], [RefId], [Debit], [Credit], [Notes], [IsActive]) VALUES (3, '2026-03-29 14:46:25.2360000', 1, 3, 1, 0.00, 500.00, N'مرتجع مبيعات رقم 1 لفاتورة 1', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerLedgers] WHERE [Id] = 4)
    INSERT INTO [dbo].[CustomerLedgers] ([Id], [Date], [CustomerId], [Type], [RefId], [Debit], [Credit], [Notes], [IsActive]) VALUES (4, '2026-03-29 14:52:41.6730000', 1, 2, 1, 0.00, 20000.00, N'سند قبض رقم 1 (رصيد دائن غير موزع: 20000)', 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerLedgers] WHERE [Id] = 5)
    INSERT INTO [dbo].[CustomerLedgers] ([Id], [Date], [CustomerId], [Type], [RefId], [Debit], [Credit], [Notes], [IsActive]) VALUES (5, '2026-03-29 14:54:15.8160000', 1, 1, 2, 500.00, 0.00, N'فاتورة بيع رقم 2', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerLedgers] WHERE [Id] = 6)
    INSERT INTO [dbo].[CustomerLedgers] ([Id], [Date], [CustomerId], [Type], [RefId], [Debit], [Credit], [Notes], [IsActive]) VALUES (6, '2026-03-29 15:08:32.2820000', 1, 2, 2, 0.00, 500.00, N'سند قبض رقم 2', 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerLedgers] WHERE [Id] = 7)
    INSERT INTO [dbo].[CustomerLedgers] ([Id], [Date], [CustomerId], [Type], [RefId], [Debit], [Credit], [Notes], [IsActive]) VALUES (7, '2026-03-29 15:40:01.0778433', 1, 2, 1, 20000.00, 0.00, N'إلغاء سند قبض رقم 1', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerLedgers] WHERE [Id] = 8)
    INSERT INTO [dbo].[CustomerLedgers] ([Id], [Date], [CustomerId], [Type], [RefId], [Debit], [Credit], [Notes], [IsActive]) VALUES (8, '2026-03-29 15:40:11.2180000', 1, 2, 3, 0.00, 500.00, N'سند قبض رقم 3', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerLedgers] WHERE [Id] = 9)
    INSERT INTO [dbo].[CustomerLedgers] ([Id], [Date], [CustomerId], [Type], [RefId], [Debit], [Credit], [Notes], [IsActive]) VALUES (9, '2026-03-29 15:41:03.4530000', 1, 2, 4, 0.00, 500.00, N'سند قبض رقم 4 (رصيد دائن غير موزع: 500.00)', 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerLedgers] WHERE [Id] = 10)
    INSERT INTO [dbo].[CustomerLedgers] ([Id], [Date], [CustomerId], [Type], [RefId], [Debit], [Credit], [Notes], [IsActive]) VALUES (10, '2026-03-29 15:50:10.2010000', 1, 1, 3, 1000.00, 0.00, N'فاتورة بيع رقم 3', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerLedgers] WHERE [Id] = 11)
    INSERT INTO [dbo].[CustomerLedgers] ([Id], [Date], [CustomerId], [Type], [RefId], [Debit], [Credit], [Notes], [IsActive]) VALUES (11, '2026-03-29 15:52:01.8210000', 1, 2, 5, 0.00, 750.00, N'سند قبض رقم 5', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerLedgers] WHERE [Id] = 12)
    INSERT INTO [dbo].[CustomerLedgers] ([Id], [Date], [CustomerId], [Type], [RefId], [Debit], [Credit], [Notes], [IsActive]) VALUES (12, '2026-03-29 15:52:15.9800000', 1, 2, 6, 0.00, 250.00, N'سند قبض رقم 6', 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerLedgers] WHERE [Id] = 13)
    INSERT INTO [dbo].[CustomerLedgers] ([Id], [Date], [CustomerId], [Type], [RefId], [Debit], [Credit], [Notes], [IsActive]) VALUES (13, '2026-03-29 15:53:48.5309239', 1, 2, 4, 500.00, 0.00, N'إلغاء سند قبض رقم 4', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerLedgers] WHERE [Id] = 14)
    INSERT INTO [dbo].[CustomerLedgers] ([Id], [Date], [CustomerId], [Type], [RefId], [Debit], [Credit], [Notes], [IsActive]) VALUES (14, '2026-03-29 15:54:11.6662237', 1, 2, 6, 250.00, 0.00, N'إلغاء سند قبض رقم 6', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerLedgers] WHERE [Id] = 15)
    INSERT INTO [dbo].[CustomerLedgers] ([Id], [Date], [CustomerId], [Type], [RefId], [Debit], [Credit], [Notes], [IsActive]) VALUES (15, '2026-03-29 15:56:08.5980000', 1, 2, 7, 0.00, 250.00, N'سند قبض رقم 7', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerLedgers] WHERE [Id] = 16)
    INSERT INTO [dbo].[CustomerLedgers] ([Id], [Date], [CustomerId], [Type], [RefId], [Debit], [Credit], [Notes], [IsActive]) VALUES (16, '2026-03-29 16:07:07.5760000', 1, 1, 4, 2500.00, 0.00, N'فاتورة بيع رقم 4', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerLedgers] WHERE [Id] = 17)
    INSERT INTO [dbo].[CustomerLedgers] ([Id], [Date], [CustomerId], [Type], [RefId], [Debit], [Credit], [Notes], [IsActive]) VALUES (17, '2026-03-29 16:07:54.3940000', 1, 2, 8, 0.00, 2000.00, N'سند قبض رقم 8', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerLedgers] WHERE [Id] = 18)
    INSERT INTO [dbo].[CustomerLedgers] ([Id], [Date], [CustomerId], [Type], [RefId], [Debit], [Credit], [Notes], [IsActive]) VALUES (18, '2026-03-29 16:08:21.0720000', 1, 2, 9, 0.00, 500.00, N'سند قبض رقم 9', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerLedgers] WHERE [Id] = 19)
    INSERT INTO [dbo].[CustomerLedgers] ([Id], [Date], [CustomerId], [Type], [RefId], [Debit], [Credit], [Notes], [IsActive]) VALUES (19, '2026-03-29 16:26:34.0900000', 4, 2, 10, 0.00, 500.00, N'سند قبض رقم 10 (رصيد دائن غير موزع: 500.00)', 0);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerLedgers] WHERE [Id] = 20)
    INSERT INTO [dbo].[CustomerLedgers] ([Id], [Date], [CustomerId], [Type], [RefId], [Debit], [Credit], [Notes], [IsActive]) VALUES (20, '2026-03-29 16:26:48.4710000', 4, 1, 5, 750.00, 0.00, N'فاتورة بيع رقم 5', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerLedgers] WHERE [Id] = 21)
    INSERT INTO [dbo].[CustomerLedgers] ([Id], [Date], [CustomerId], [Type], [RefId], [Debit], [Credit], [Notes], [IsActive]) VALUES (21, '2026-03-29 16:26:48.4710000', 4, 2, 5, 0.00, 750.00, N'تحصيل داخل فاتورة بيع رقم 5', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerLedgers] WHERE [Id] = 22)
    INSERT INTO [dbo].[CustomerLedgers] ([Id], [Date], [CustomerId], [Type], [RefId], [Debit], [Credit], [Notes], [IsActive]) VALUES (22, '2026-03-29 16:28:01.8220000', 4, 1, 6, 750.00, 0.00, N'فاتورة بيع رقم 6', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerLedgers] WHERE [Id] = 23)
    INSERT INTO [dbo].[CustomerLedgers] ([Id], [Date], [CustomerId], [Type], [RefId], [Debit], [Credit], [Notes], [IsActive]) VALUES (23, '2026-03-29 16:34:03.6490000', 4, 2, 11, 0.00, 750.00, N'سند قبض رقم 11', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerLedgers] WHERE [Id] = 24)
    INSERT INTO [dbo].[CustomerLedgers] ([Id], [Date], [CustomerId], [Type], [RefId], [Debit], [Credit], [Notes], [IsActive]) VALUES (24, '2026-03-29 16:37:11.0200000', 4, 1, 9, 500.00, 0.00, N'فاتورة بيع رقم 9', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerLedgers] WHERE [Id] = 25)
    INSERT INTO [dbo].[CustomerLedgers] ([Id], [Date], [CustomerId], [Type], [RefId], [Debit], [Credit], [Notes], [IsActive]) VALUES (25, '2026-03-29 16:37:59.3460000', 4, 1, 10, 2500.00, 0.00, N'فاتورة بيع رقم 10', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerLedgers] WHERE [Id] = 26)
    INSERT INTO [dbo].[CustomerLedgers] ([Id], [Date], [CustomerId], [Type], [RefId], [Debit], [Credit], [Notes], [IsActive]) VALUES (26, '2026-03-29 16:38:52.0790000', 4, 2, 12, 0.00, 2500.00, N'سند قبض رقم 12', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[CustomerLedgers] WHERE [Id] = 27)
    INSERT INTO [dbo].[CustomerLedgers] ([Id], [Date], [CustomerId], [Type], [RefId], [Debit], [Credit], [Notes], [IsActive]) VALUES (27, '2026-03-29 16:39:43.3020978', 4, 2, 10, 500.00, 0.00, N'إلغاء سند قبض رقم 10', 1);
SET IDENTITY_INSERT [dbo].[CustomerLedgers] OFF;

-- [StockMovements] rows: 12
SET IDENTITY_INSERT [dbo].[StockMovements] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[StockMovements] WHERE [Id] = 1)
    INSERT INTO [dbo].[StockMovements] ([Id], [Date], [ItemId], [BatchId], [QtyIn], [QtyOut], [UnitCost], [RefType], [RefId], [Notes]) VALUES (1, '2026-03-29 14:39:25.0010000', 1, 1, 2.00, 0.00, 200.00, 1, 1, N'فاتورة شراء رقم: 1');
IF NOT EXISTS (SELECT 1 FROM [dbo].[StockMovements] WHERE [Id] = 2)
    INSERT INTO [dbo].[StockMovements] ([Id], [Date], [ItemId], [BatchId], [QtyIn], [QtyOut], [UnitCost], [RefType], [RefId], [Notes]) VALUES (2, '2026-03-29 14:45:11.4500000', 1, 1, 0.00, 2.00, 200.00, 3, 1, N'فاتورة بيع رقم: 1');
IF NOT EXISTS (SELECT 1 FROM [dbo].[StockMovements] WHERE [Id] = 3)
    INSERT INTO [dbo].[StockMovements] ([Id], [Date], [ItemId], [BatchId], [QtyIn], [QtyOut], [UnitCost], [RefType], [RefId], [Notes]) VALUES (3, '2026-03-29 14:46:25.2360000', 1, 1, 2.00, 0.00, 0.00, 4, 1, N'مرتجع مبيعات لفاتورة: 1');
IF NOT EXISTS (SELECT 1 FROM [dbo].[StockMovements] WHERE [Id] = 4)
    INSERT INTO [dbo].[StockMovements] ([Id], [Date], [ItemId], [BatchId], [QtyIn], [QtyOut], [UnitCost], [RefType], [RefId], [Notes]) VALUES (4, '2026-03-29 14:54:15.8160000', 1, 1, 0.00, 2.00, 200.00, 3, 2, N'فاتورة بيع رقم: 2');
IF NOT EXISTS (SELECT 1 FROM [dbo].[StockMovements] WHERE [Id] = 5)
    INSERT INTO [dbo].[StockMovements] ([Id], [Date], [ItemId], [BatchId], [QtyIn], [QtyOut], [UnitCost], [RefType], [RefId], [Notes]) VALUES (5, '2026-03-29 15:04:49.5330000', 1, 2, 20.00, 0.00, 100.00, 1, 2, N'فاتورة شراء رقم: 2');
IF NOT EXISTS (SELECT 1 FROM [dbo].[StockMovements] WHERE [Id] = 6)
    INSERT INTO [dbo].[StockMovements] ([Id], [Date], [ItemId], [BatchId], [QtyIn], [QtyOut], [UnitCost], [RefType], [RefId], [Notes]) VALUES (6, '2026-03-29 15:50:10.2010000', 1, 2, 0.00, 4.00, 100.00, 3, 3, N'فاتورة بيع رقم: 3');
IF NOT EXISTS (SELECT 1 FROM [dbo].[StockMovements] WHERE [Id] = 7)
    INSERT INTO [dbo].[StockMovements] ([Id], [Date], [ItemId], [BatchId], [QtyIn], [QtyOut], [UnitCost], [RefType], [RefId], [Notes]) VALUES (7, '2026-03-29 16:07:07.5760000', 1, 2, 0.00, 10.00, 100.00, 3, 4, N'فاتورة بيع رقم: 4');
IF NOT EXISTS (SELECT 1 FROM [dbo].[StockMovements] WHERE [Id] = 8)
    INSERT INTO [dbo].[StockMovements] ([Id], [Date], [ItemId], [BatchId], [QtyIn], [QtyOut], [UnitCost], [RefType], [RefId], [Notes]) VALUES (8, '2026-03-29 16:26:48.4710000', 1, 2, 0.00, 3.00, 100.00, 3, 5, N'فاتورة بيع رقم: 5');
IF NOT EXISTS (SELECT 1 FROM [dbo].[StockMovements] WHERE [Id] = 9)
    INSERT INTO [dbo].[StockMovements] ([Id], [Date], [ItemId], [BatchId], [QtyIn], [QtyOut], [UnitCost], [RefType], [RefId], [Notes]) VALUES (9, '2026-03-29 16:28:01.8220000', 1, 2, 0.00, 3.00, 100.00, 3, 6, N'فاتورة بيع رقم: 6');
IF NOT EXISTS (SELECT 1 FROM [dbo].[StockMovements] WHERE [Id] = 10)
    INSERT INTO [dbo].[StockMovements] ([Id], [Date], [ItemId], [BatchId], [QtyIn], [QtyOut], [UnitCost], [RefType], [RefId], [Notes]) VALUES (10, '2026-03-29 16:36:43.4480000', 1, 3, 100.00, 0.00, 500.00, 1, 3, N'فاتورة شراء رقم: 3');
IF NOT EXISTS (SELECT 1 FROM [dbo].[StockMovements] WHERE [Id] = 11)
    INSERT INTO [dbo].[StockMovements] ([Id], [Date], [ItemId], [BatchId], [QtyIn], [QtyOut], [UnitCost], [RefType], [RefId], [Notes]) VALUES (11, '2026-03-29 16:37:11.0200000', 1, 3, 0.00, 2.00, 500.00, 3, 9, N'فاتورة بيع رقم: 9');
IF NOT EXISTS (SELECT 1 FROM [dbo].[StockMovements] WHERE [Id] = 12)
    INSERT INTO [dbo].[StockMovements] ([Id], [Date], [ItemId], [BatchId], [QtyIn], [QtyOut], [UnitCost], [RefType], [RefId], [Notes]) VALUES (12, '2026-03-29 16:37:59.3460000', 1, 3, 0.00, 10.00, 500.00, 3, 10, N'فاتورة بيع رقم: 10');
SET IDENTITY_INSERT [dbo].[StockMovements] OFF;

    COMMIT;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    THROW;
END CATCH;
