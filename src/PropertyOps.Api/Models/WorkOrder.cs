namespace PropertyOps.Api.Models;

public class WorkOrder
{
    public int Id { get; set; }

    public string ExternalWorkOrderId { get; set; } = string.Empty;

    public int PropertyId { get; set; }

    public Property? Property { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Priority { get; set; } = "Normal";

    public string Status { get; set; } = "Open";

    public DateTime OpenedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public decimal EstimatedCost { get; set; }
}
