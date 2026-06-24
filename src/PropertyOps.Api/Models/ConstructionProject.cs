namespace PropertyOps.Api.Models;

public class ConstructionProject
{
    public int Id { get; set; }

    public int? PropertyId { get; set; }

    public Property? Property { get; set; }

    public string ProjectCode { get; set; } = string.Empty;

    public string ProjectName { get; set; } = string.Empty;

    public decimal ApprovedBudget { get; set; }

    public decimal ActualCost { get; set; }

    public decimal PercentComplete { get; set; }

    public string Status { get; set; } = "In Progress";

    public DateTime TargetCompletionDate { get; set; }
}