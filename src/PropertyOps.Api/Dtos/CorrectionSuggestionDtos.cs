namespace PropertyOps.Api.Dtos;

public record CorrectionSuggestionResponse(
    int Id,
    int DataQualityErrorId,
    string SourceRecordId,
    string FieldName,
    string OriginalValue,
    string? SuggestedValue,
    decimal Confidence,
    string Reason,
    string Status,
    string ModelName,
    string PromptVersion,
    DateTime CreatedAtUtc,
    DateTime? ReviewedAtUtc,
    string? ReviewedBy,
    string? ReviewerNotes
);

public record ReviewCorrectionSuggestionRequest(
    string ReviewerName,
    string? ReviewerNotes
);