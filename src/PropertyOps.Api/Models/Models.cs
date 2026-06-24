namespace PropertyOps.Api.Models;

public class Lease
{
    public int Id { get; set; }

    public string ExternalLeaseId { get; set; } = string.Empty;

    public int PropertyId { get; set; }

    public Property? Property { get; set; }

    public string UnitNumber { get; set; } = string.Empty;

    public string ResidentName { get; set; } = string.Empty;

    public decimal MonthlyRent { get; set; }

    public string Status { get; set; } = "Active";

    public DateTime LeaseStartDate { get; set; }

    public DateTime LeaseEndDate { get; set; }

    public DateTime? MoveInDate { get; set; }

    public DateTime? MoveOutDate { get; set; }
}