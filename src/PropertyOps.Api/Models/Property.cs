namespace PropertyOps.Api.Models; // address label in folder where blueprint lives

public class Property
{
    public int Id { get; set; }

    public string PropertyCode { get; set; } = string.Empty; // empty not zero cause it can stay blank until we writes on it

    public string Name { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public int TotalUnits { get; set; }

    public ICollection<Lease> Leases { get; set; } = new List<Lease>();

    public ICollection<ConstructionProject> ConstructionProjects { get; set; } = new List<ConstructionProject>();

    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}