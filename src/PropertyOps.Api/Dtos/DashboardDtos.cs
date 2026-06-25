namespace PropertyOps.Api.Dtos;

//These are not database tables. They are shapes for the reporting data returned by SQL.
public class PortfolioSummaryRow
{
    public int TotalProperties { get; set; }
    public int TotalUnits { get; set; }
    public int ActiveLeases { get; set; }
    public int OccupiedUnits { get; set; }
    public decimal OccupancyRate { get; set; }
    public decimal MonthlyScheduledRent { get; set; }
    public int OpenWorkOrders { get; set; }
    public decimal AverageOpenWorkOrderAgeDays { get; set; }
    public int ProjectsOverBudget { get; set; }
    public decimal TotalConstructionBudget { get; set; }
    public decimal TotalConstructionActualCost { get; set; }
}

public class PropertyPerformanceRow
{
    public int PropertyId { get; set; }
    public string PropertyCode { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public int TotalUnits { get; set; }
    public int ActiveLeases { get; set; }
    public int OccupiedUnits { get; set; }
    public decimal OccupancyRate { get; set; }
    public decimal MonthlyScheduledRent { get; set; }
    public int OpenWorkOrders { get; set; }
    public decimal AverageOpenWorkOrderAgeDays { get; set; }
}

public class ConstructionRiskRow
{
    public int ProjectId { get; set; }
    public string PropertyCode { get; set; } = string.Empty;
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public decimal ApprovedBudget { get; set; }
    public decimal ActualCost { get; set; }
    public decimal BudgetVariance { get; set; }
    public decimal BudgetVariancePercent { get; set; }
    public decimal PercentComplete { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime TargetCompletionDate { get; set; }
}

public class MaintenanceAlertRow
{
    public int WorkOrderId { get; set; }
    public string ExternalWorkOrderId { get; set; } = string.Empty;
    public string PropertyCode { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime OpenedAt { get; set; }
    public int AgeDays { get; set; }
    public decimal EstimatedCost { get; set; }
}