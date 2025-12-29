using Microsoft.EntityFrameworkCore;
using SmartFactory.Application.Entities;

namespace SmartFactory.Application.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Core System
    public DbSet<User> Users { get; set; } = null!;
    
    // PO Management System
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; } = null!;
    public DbSet<POProduct> POProducts { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Part> Parts { get; set; } = null!;
    
    // Processing Configuration
    public DbSet<ProcessingType> ProcessingTypes { get; set; } = null!;
    public DbSet<ProcessMethod> ProcessMethods { get; set; } = null!;
    public DbSet<PartProcessingType> PartProcessingTypes { get; set; } = null!;
    public DbSet<ExcelMapping> ExcelMappings { get; set; } = null!;
    
    // PO Operations (tính tiền)
    public DbSet<POOperation> POOperations { get; set; } = null!;
    
    // Production Operations (thực tế)
    public DbSet<ProductionOperation> ProductionOperations { get; set; } = null!;
    public DbSet<MappingPOProduction> MappingPOProductions { get; set; } = null!;
    
    // Resources
    public DbSet<Material> Materials { get; set; } = null!;
    public DbSet<MaterialReceipt> MaterialReceipts { get; set; } = null!;
    public DbSet<Tool> Tools { get; set; } = null!;
    public DbSet<Machine> Machines { get; set; } = null!;
    public DbSet<ProductionOperationMaterial> ProductionOperationMaterials { get; set; } = null!;
    
    // Phase 1: PO Material Baseline & Process BOM
    public DbSet<POMaterialBaseline> POMaterialBaselines { get; set; } = null!;
    public DbSet<ProcessBOM> ProcessBOMs { get; set; } = null!;
    public DbSet<ProcessBOMDetail> ProcessBOMDetails { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUserEntity(modelBuilder);
        ConfigureCustomerEntity(modelBuilder);
        ConfigurePurchaseOrderEntity(modelBuilder);
        ConfigurePOProductEntity(modelBuilder);
        ConfigureProductEntity(modelBuilder);
        ConfigurePartEntity(modelBuilder);
        ConfigureProcessingTypeEntity(modelBuilder);
        ConfigureProcessMethodEntity(modelBuilder);
        ConfigurePartProcessingTypeEntity(modelBuilder);
        ConfigurePOOperationEntity(modelBuilder);
        ConfigureProductionOperationEntity(modelBuilder);
        ConfigureMappingPOProductionEntity(modelBuilder);
        ConfigureMaterialEntity(modelBuilder);
        ConfigureMaterialReceiptEntity(modelBuilder);
        ConfigureToolEntity(modelBuilder);
        ConfigureMachineEntity(modelBuilder);
        ConfigureProductionOperationMaterialEntity(modelBuilder);
        ConfigureExcelMappingEntity(modelBuilder);
        
        // Phase 1 configurations
        ConfigurePOMaterialBaselineEntity(modelBuilder);
        ConfigureProcessBOMEntity(modelBuilder);
        ConfigureProcessBOMDetailEntity(modelBuilder);
    }

    private void ConfigureUserEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }

    private void ConfigureCustomerEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.ContactPerson).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.PaymentTerms).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }

    private void ConfigurePurchaseOrderEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.PONumber).IsUnique();
            entity.Property(e => e.PONumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Version).IsRequired().HasMaxLength(10);
            entity.Property(e => e.ProcessingType).HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.PurchaseOrders)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.OriginalPO)
                .WithMany(p => p.DerivedVersions)
                .HasForeignKey(e => e.OriginalPOId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigurePOProductEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<POProduct>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.PurchaseOrder)
                .WithMany(p => p.POProducts)
                .HasForeignKey(e => e.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.POProducts)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureProductEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }

    private void ConfigurePartEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Part>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Position).HasMaxLength(50);
            entity.Property(e => e.Material).HasMaxLength(50);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.Weight).HasColumnType("decimal(18,3)");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Product)
                .WithMany(p => p.Parts)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureProcessingTypeEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProcessingType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }

    private void ConfigureProcessMethodEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProcessMethod>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.ProcessingType)
                .WithMany(p => p.ProcessMethods)
                .HasForeignKey(e => e.ProcessingTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigurePartProcessingTypeEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PartProcessingType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            // Create unique index on (PartId + ProcessingTypeId) to prevent duplicates
            entity.HasIndex(e => new { e.PartId, e.ProcessingTypeId }).IsUnique();
            
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Part)
                .WithMany(p => p.PartProcessingTypes)
                .HasForeignKey(e => e.PartId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ProcessingType)
                .WithMany(p => p.PartProcessingTypes)
                .HasForeignKey(e => e.ProcessingTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigurePOOperationEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<POOperation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.OperationName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CycleTime).HasColumnType("decimal(10,2)");
            entity.Property(e => e.SprayPosition).HasMaxLength(100);
            entity.Property(e => e.PrintContent).HasMaxLength(255);
            entity.Property(e => e.AssemblyContent).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.PurchaseOrder)
                .WithMany(p => p.POOperations)
                .HasForeignKey(e => e.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Part)
                .WithMany(p => p.POOperations)
                .HasForeignKey(e => e.PartId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ProcessingType)
                .WithMany()
                .HasForeignKey(e => e.ProcessingTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ProcessMethod)
                .WithMany()
                .HasForeignKey(e => e.ProcessMethodId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureProductionOperationEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductionOperation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.OperationName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CycleTime).HasColumnType("decimal(10,2)");
            entity.Property(e => e.AssignedTo).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.PurchaseOrder)
                .WithMany()
                .HasForeignKey(e => e.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Part)
                .WithMany()
                .HasForeignKey(e => e.PartId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ProcessMethod)
                .WithMany()
                .HasForeignKey(e => e.ProcessMethodId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Machine)
                .WithMany(m => m.ProductionOperations)
                .HasForeignKey(e => e.MachineId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Tool)
                .WithMany(t => t.ProductionOperations)
                .HasForeignKey(e => e.ToolId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Material)
                .WithMany(m => m.ProductionOperations)
                .HasForeignKey(e => e.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureMappingPOProductionEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MappingPOProduction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.AllocationRatio).HasColumnType("decimal(10,4)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.POOperation)
                .WithMany(p => p.MappingPOProductions)
                .HasForeignKey(e => e.POOperationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ProductionOperation)
                .WithMany(p => p.MappingPOProductions)
                .HasForeignKey(e => e.ProductionOperationId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureMaterialEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Material>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ColorCode).HasMaxLength(50);
            entity.Property(e => e.Supplier).HasMaxLength(255);
            entity.Property(e => e.Unit).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CurrentStock).HasColumnType("decimal(18,3)");
            entity.Property(e => e.MinStock).HasColumnType("decimal(18,3)");
            entity.Property(e => e.UnitCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Materials)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureMaterialReceiptEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MaterialReceipt>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.ReceiptNumber).IsUnique();
            entity.Property(e => e.WarehouseCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18,3)");
            entity.Property(e => e.Unit).IsRequired().HasMaxLength(20);
            entity.Property(e => e.BatchNumber).HasMaxLength(100);
            entity.Property(e => e.SupplierCode).HasMaxLength(100);
            entity.Property(e => e.PurchasePOCode).HasMaxLength(100);
            entity.Property(e => e.ReceiptNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.MaterialReceipts)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Material)
                .WithMany(m => m.MaterialReceipts)
                .HasForeignKey(e => e.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureToolEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tool>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureMachineEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Machine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Model).HasMaxLength(100);
            entity.Property(e => e.Manufacturer).HasMaxLength(100);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.MaxCapacity).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }

    private void ConfigureProductionOperationMaterialEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductionOperationMaterial>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.QuantityRequired).HasColumnType("decimal(18,3)");
            entity.Property(e => e.QuantityUsed).HasColumnType("decimal(18,3)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.ProductionOperation)
                .WithMany(p => p.ProductionOperationMaterials)
                .HasForeignKey(e => e.ProductionOperationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Material)
                .WithMany(m => m.ProductionOperationMaterials)
                .HasForeignKey(e => e.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureExcelMappingEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExcelMapping>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.TemplateType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TemplateName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ExcelColumnName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ColumnPosition).HasMaxLength(10);
            entity.Property(e => e.SystemFieldName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DataType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.DefaultValue).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }
    
    // Phase 1 Entity Configurations
    
    private void ConfigurePOMaterialBaselineEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<POMaterialBaseline>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.MaterialCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.MaterialName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CommittedQuantity).HasColumnType("decimal(18,3)");
            entity.Property(e => e.Unit).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ProductCode).HasMaxLength(50);
            entity.Property(e => e.PartCode).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasOne(e => e.PurchaseOrder)
                .WithMany(p => p.MaterialBaselines)
                .HasForeignKey(e => e.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
    
    private void ConfigureProcessBOMEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProcessBOM>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Version).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Create unique index on (PartId + ProcessingTypeId + Version)
            entity.HasIndex(e => new { e.PartId, e.ProcessingTypeId, e.Version }).IsUnique();
            
            entity.HasOne(e => e.Part)
                .WithMany()
                .HasForeignKey(e => e.PartId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.ProcessingType)
                .WithMany()
                .HasForeignKey(e => e.ProcessingTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
    
    private void ConfigureProcessBOMDetailEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProcessBOMDetail>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.MaterialCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.MaterialName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.QuantityPerUnit).HasColumnType("decimal(18,6)");
            entity.Property(e => e.ScrapRate).HasColumnType("decimal(10,4)");
            entity.Property(e => e.Unit).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ProcessStep).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasOne(e => e.ProcessBOM)
                .WithMany(p => p.BOMDetails)
                .HasForeignKey(e => e.ProcessBOMId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

