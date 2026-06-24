namespace PropertyOps.Api.Models;

public class PipelineRun
{
    public int Id { get; set; }

    public string PipelineName { get; set; } = string.Empty;

    public string Status { get; set; } = "Running";

    public DateTime StartedAtUtc { get; set; }

    public DateTime? FinishedAtUtc { get; set; }

    public int RecordsReceived { get; set; }

    public int RecordsLoaded { get; set; }

    public int RecordsRejected { get; set; }

    public string? ErrorMessage { get; set; }

    public ICollection<DataQualityError> DataQualityErrors { get; set; } = new List<DataQualityError>();
}