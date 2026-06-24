namespace PropertyOps.Api.Models;

public class DataQualityError
{
    public int Id { get; set; }

    public int PipelineRunId { get; set; }

    public PipelineRun? PipelineRun { get; set; }

    public string SourceRecordId { get; set; } = string.Empty;

    public string FieldName { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public string? RawData { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}