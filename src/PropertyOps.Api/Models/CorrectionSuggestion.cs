namespace PropertyOps.Api.Models;

public class CorrectionSuggestion
{
    public int Id { get; set; }

    public int DataQualityErrorId { get; set; }

    public DataQualityError? DataQualityError { get; set; }

    public string FieldName { get; set; } = string.Empty;

    public string OriginalValue { get; set; } = string.Empty;

    public string? SuggestedValue { get; set; }

    public decimal Confidence { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string Status { get; set; } = "PendingReview";
    // PendingReview, Approved, Rejected, Applied

    public string ModelName { get; set; } = string.Empty;

    public string PromptVersion { get; set; } = "v1";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ReviewedAtUtc { get; set; }

    public string? ReviewedBy { get; set; }

    public string? ReviewerNotes { get; set; }
}