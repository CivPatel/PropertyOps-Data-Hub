namespace PropertyOps.Api.Dtos;

public record PipelineRunResponse(
    int Id,
    string PipelineName,
    string Status,
    DateTime StartedAtUtc,
    DateTime? FinishedAtUtc,
    int RecordsReceived,
    int RecordsLoaded,
    int RecordsRejected,
    string? ErrorMessage
);

public record DataQualityErrorResponse(
    int Id,
    int PipelineRunId,
    string SourceRecordId,
    string FieldName,
    string ErrorMessage,
    string? RawData,
    DateTime CreatedAtUtc
);