-- PMC Planning System Migration Script
-- Created: 2026-01-12
-- Description: Creates tables for PMC Planning feature with version control

-- Table: PMCWeeks
-- Stores PMC planning weeks with version control
CREATE TABLE [dbo].[PMCWeeks] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [WeekStartDate] DATE NOT NULL,
    [WeekEndDate] DATE NOT NULL,
    [Version] INT NOT NULL,
    [WeekName] NVARCHAR(100) NOT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [Status] NVARCHAR(20) NOT NULL DEFAULT 'DRAFT',
    [Notes] NVARCHAR(1000) NULL,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NULL,
    
    -- Foreign Keys
    CONSTRAINT [FK_PMCWeeks_Users_CreatedBy] FOREIGN KEY ([CreatedBy]) 
        REFERENCES [dbo].[Users] ([Id]) ON DELETE NO ACTION,
    
    -- Indexes
    CONSTRAINT [IX_PMCWeeks_WeekStartDate_Version] UNIQUE ([WeekStartDate], [Version])
);

CREATE INDEX [IX_PMCWeeks_WeekStartDate] ON [dbo].[PMCWeeks] ([WeekStartDate]);
CREATE INDEX [IX_PMCWeeks_IsActive] ON [dbo].[PMCWeeks] ([IsActive]);

GO

-- Table: PMCRows
-- Stores individual planning rows for each product-component-plantype combination
CREATE TABLE [dbo].[PMCRows] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [PMCWeekId] UNIQUEIDENTIFIER NOT NULL,
    [ProductCode] NVARCHAR(100) NOT NULL,
    [ComponentName] NVARCHAR(200) NOT NULL,
    [CustomerId] UNIQUEIDENTIFIER NULL,
    [CustomerName] NVARCHAR(200) NULL,
    [PlanType] NVARCHAR(50) NOT NULL, -- REQUIREMENT, PRODUCTION, CLAMP
    [DisplayOrder] INT NOT NULL DEFAULT 0,
    [TotalValue] DECIMAL(18,3) NULL,
    [RowGroup] NVARCHAR(300) NOT NULL,
    [Notes] NVARCHAR(1000) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    -- Foreign Keys
    CONSTRAINT [FK_PMCRows_PMCWeeks_PMCWeekId] FOREIGN KEY ([PMCWeekId]) 
        REFERENCES [dbo].[PMCWeeks] ([Id]) ON DELETE CASCADE,
    
    CONSTRAINT [FK_PMCRows_Customers_CustomerId] FOREIGN KEY ([CustomerId]) 
        REFERENCES [dbo].[Customers] ([Id]) ON DELETE SET NULL,
    
    -- Indexes
    INDEX [IX_PMCRows_PMCWeekId] ([PMCWeekId]),
    INDEX [IX_PMCRows_Composite] ([PMCWeekId], [ProductCode], [ComponentName], [PlanType])
);

GO

-- Table: PMCCells
-- Stores individual cell values for each date in the planning week
CREATE TABLE [dbo].[PMCCells] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [PMCRowId] UNIQUEIDENTIFIER NOT NULL,
    [WorkDate] DATE NOT NULL,
    [Value] DECIMAL(18,3) NOT NULL DEFAULT 0,
    [IsEditable] BIT NOT NULL DEFAULT 1,
    [BackgroundColor] NVARCHAR(50) NULL,
    [Notes] NVARCHAR(500) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NULL,
    
    -- Foreign Keys
    CONSTRAINT [FK_PMCCells_PMCRows_PMCRowId] FOREIGN KEY ([PMCRowId]) 
        REFERENCES [dbo].[PMCRows] ([Id]) ON DELETE CASCADE,
    
    -- Indexes
    CONSTRAINT [IX_PMCCells_PMCRowId_WorkDate] UNIQUE ([PMCRowId], [WorkDate])
);

CREATE INDEX [IX_PMCCells_PMCRowId] ON [dbo].[PMCCells] ([PMCRowId]);
CREATE INDEX [IX_PMCCells_WorkDate] ON [dbo].[PMCCells] ([WorkDate]);

GO

PRINT 'PMC Planning tables created successfully';
