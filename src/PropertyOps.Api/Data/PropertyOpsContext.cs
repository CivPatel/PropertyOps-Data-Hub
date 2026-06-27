using Microsoft.EntityFrameworkCore;
using PropertyOps.Api.Models;

namespace PropertyOps.Api.Data;

public class PropertyOpsDbContext : DbContext
{
    public PropertyOpsDbContext(DbContextOptions<PropertyOpsDbContext> options)
        : base(options)
    {
    }

    public DbSet<CorrectionSuggestion> CorrectionSuggestions
    => Set<CorrectionSuggestion>();
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Lease> Leases => Set<Lease>();
    public DbSet<ConstructionProject> ConstructionProjects => Set<ConstructionProject>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<PipelineRun> PipelineRuns => Set<PipelineRun>();
    public DbSet<DataQualityError> DataQualityErrors => Set<DataQualityError>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Property>(entity =>
        {
            entity.Property(x => x.PropertyCode).HasMaxLength(25).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.City).HasMaxLength(80).IsRequired();
            entity.Property(x => x.State).HasMaxLength(2).IsRequired();

            entity.HasIndex(x => x.PropertyCode).IsUnique();
        });

        modelBuilder.Entity<Lease>(entity =>
        {
            entity.Property(x => x.ExternalLeaseId).HasMaxLength(50).IsRequired();
            entity.Property(x => x.UnitNumber).HasMaxLength(20).IsRequired();
            entity.Property(x => x.ResidentName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.Property(x => x.MonthlyRent).HasPrecision(12, 2);

            entity.HasIndex(x => x.ExternalLeaseId).IsUnique();
            entity.HasIndex(x => new { x.PropertyId, x.Status });

            entity.HasOne(x => x.Property)
                .WithMany(x => x.Leases)
                .HasForeignKey(x => x.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ConstructionProject>(entity =>
        {
            entity.Property(x => x.ProjectCode).HasMaxLength(30).IsRequired();
            entity.Property(x => x.ProjectName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.Property(x => x.ApprovedBudget).HasPrecision(14, 2);
            entity.Property(x => x.ActualCost).HasPrecision(14, 2);
            entity.Property(x => x.PercentComplete).HasPrecision(5, 2);

            entity.HasIndex(x => x.ProjectCode).IsUnique();

            entity.HasOne(x => x.Property)
                .WithMany(x => x.ConstructionProjects)
                .HasForeignKey(x => x.PropertyId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WorkOrder>(entity =>
        {
            entity.Property(x => x.ExternalWorkOrderId).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Priority).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.Property(x => x.EstimatedCost).HasPrecision(12, 2);

            entity.HasIndex(x => x.ExternalWorkOrderId).IsUnique();
            entity.HasIndex(x => new { x.PropertyId, x.Status });

            entity.HasOne(x => x.Property)
                .WithMany(x => x.WorkOrders)
                .HasForeignKey(x => x.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PipelineRun>(entity =>
        {
            entity.Property(x => x.PipelineName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();

            entity.HasIndex(x => new { x.PipelineName, x.StartedAtUtc });
        });
        
        modelBuilder.Entity<DataQualityError>(entity =>
        {
            entity.Property(x => x.SourceRecordId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.FieldName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ErrorMessage).HasMaxLength(1000).IsRequired();

            entity.HasOne(x => x.PipelineRun)
                .WithMany(x => x.DataQualityErrors)
                .HasForeignKey(x => x.PipelineRunId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<CorrectionSuggestion>(entity =>
{
    entity.Property(x => x.FieldName)
        .HasMaxLength(100)
        .IsRequired();

    entity.Property(x => x.OriginalValue)
        .HasMaxLength(500)
        .IsRequired();

    entity.Property(x => x.SuggestedValue)
        .HasMaxLength(500);

    entity.Property(x => x.Confidence)
        .HasPrecision(5, 4);

    entity.Property(x => x.Reason)
        .HasMaxLength(1000)
        .IsRequired();

    entity.Property(x => x.Status)
        .HasMaxLength(30)
        .IsRequired();

    entity.Property(x => x.ModelName)
        .HasMaxLength(100)
        .IsRequired();

    entity.Property(x => x.PromptVersion)
        .HasMaxLength(30)
        .IsRequired();

    entity.Property(x => x.ReviewedBy)
        .HasMaxLength(150);

    entity.Property(x => x.ReviewerNotes)
        .HasMaxLength(1000);

    entity.HasIndex(x => new { x.Status, x.CreatedAtUtc });
    
    entity.HasIndex(x => x.DataQualityErrorId)
        .IsUnique();
    entity.HasOne(x => x.DataQualityError)
        .WithMany(x => x.CorrectionSuggestions)
        .HasForeignKey(x => x.DataQualityErrorId)
        .OnDelete(DeleteBehavior.Cascade);
        });
        
    }
}