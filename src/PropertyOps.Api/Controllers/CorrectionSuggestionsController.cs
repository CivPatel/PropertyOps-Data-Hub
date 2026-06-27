using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropertyOps.Api.Data;
using PropertyOps.Api.Dtos;
using PropertyOps.Api.Services;

namespace PropertyOps.Api.Controllers;

[ApiController]
[Route("api/correction-suggestions")]
public class CorrectionSuggestionsController : ControllerBase
{
    private readonly PropertyOpsDbContext _db;
    private readonly ApprovedCorrectionApplicationService _applicationService;

    public CorrectionSuggestionsController(
        PropertyOpsDbContext db,
        ApprovedCorrectionApplicationService applicationService)
    {
        _db = db;
        _applicationService = applicationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CorrectionSuggestionResponse>>> GetAll(
        [FromQuery] string? status = null,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 250);

        var query = _db.CorrectionSuggestions
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(item => item.Status == status.Trim());
        }

        var suggestions = await query
            .OrderByDescending(item => item.CreatedAtUtc)
            .Take(limit)
            .Select(item => new CorrectionSuggestionResponse(
                item.Id,
                item.DataQualityErrorId,
                item.DataQualityError!.SourceRecordId,
                item.FieldName,
                item.OriginalValue,
                item.SuggestedValue,
                item.Confidence,
                item.Reason,
                item.Status,
                item.ModelName,
                item.PromptVersion,
                item.CreatedAtUtc,
                item.ReviewedAtUtc,
                item.ReviewedBy,
                item.ReviewerNotes,
                item.AppliedAtUtc,
                item.AppliedBy,
                item.AppliedLeaseId
            ))
            .ToListAsync(cancellationToken);

        return Ok(suggestions);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CorrectionSuggestionResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var suggestion = await _db.CorrectionSuggestions
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new CorrectionSuggestionResponse(
                item.Id,
                item.DataQualityErrorId,
                item.DataQualityError!.SourceRecordId,
                item.FieldName,
                item.OriginalValue,
                item.SuggestedValue,
                item.Confidence,
                item.Reason,
                item.Status,
                item.ModelName,
                item.PromptVersion,
                item.CreatedAtUtc,
                item.ReviewedAtUtc,
                item.ReviewedBy,
                item.ReviewerNotes,
                item.AppliedAtUtc,
                item.AppliedBy,
                item.AppliedLeaseId
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (suggestion is null)
        {
            return NotFound(new
            {
                message = $"Correction suggestion with ID {id} was not found."
            });
        }

        return Ok(suggestion);
    }

    [HttpPost("{id:int}/approve")]
    public async Task<ActionResult<CorrectionSuggestionResponse>> Approve(
        int id,
        ReviewCorrectionSuggestionRequest request,
        CancellationToken cancellationToken)
    {
        return await ReviewAsync(id, request, "Approved", cancellationToken);
    }

    [HttpPost("{id:int}/reject")]
    public async Task<ActionResult<CorrectionSuggestionResponse>> Reject(
        int id,
        ReviewCorrectionSuggestionRequest request,
        CancellationToken cancellationToken)
    {
        return await ReviewAsync(id, request, "Rejected", cancellationToken);
    }

    [HttpPost("{id:int}/apply")]
    public async Task<IActionResult> Apply(
        int id,
        ApplyCorrectionSuggestionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var suggestion = await _applicationService.ApplyAsync(
                id,
                request.AppliedBy,
                cancellationToken
            );

            return Ok(new
            {
                message = "Approved correction was applied successfully.",
                suggestionId = suggestion.Id,
                suggestion.Status,
                suggestion.AppliedLeaseId,
                suggestion.AppliedAtUtc,
                suggestion.AppliedBy
            });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    private async Task<ActionResult<CorrectionSuggestionResponse>> ReviewAsync(
        int id,
        ReviewCorrectionSuggestionRequest request,
        string newStatus,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ReviewerName))
        {
            return BadRequest(new
            {
                message = "ReviewerName is required."
            });
        }

        var suggestion = await _db.CorrectionSuggestions
            .Include(item => item.DataQualityError)
            .FirstOrDefaultAsync(
                item => item.Id == id,
                cancellationToken
            );

        if (suggestion is null)
        {
            return NotFound(new
            {
                message = $"Correction suggestion with ID {id} was not found."
            });
        }

        if (suggestion.Status != "PendingReview")
        {
            return Conflict(new
            {
                message = $"Only PendingReview suggestions can be reviewed. Current status: {suggestion.Status}."
            });
        }

        suggestion.Status = newStatus;
        suggestion.ReviewedAtUtc = DateTime.UtcNow;
        suggestion.ReviewedBy = request.ReviewerName.Trim();
        suggestion.ReviewerNotes = request.ReviewerNotes?.Trim();

        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new CorrectionSuggestionResponse(
            suggestion.Id,
            suggestion.DataQualityErrorId,
            suggestion.DataQualityError!.SourceRecordId,
            suggestion.FieldName,
            suggestion.OriginalValue,
            suggestion.SuggestedValue,
            suggestion.Confidence,
            suggestion.Reason,
            suggestion.Status,
            suggestion.ModelName,
            suggestion.PromptVersion,
            suggestion.CreatedAtUtc,
            suggestion.ReviewedAtUtc,
            suggestion.ReviewedBy,
            suggestion.ReviewerNotes,
            suggestion.AppliedAtUtc,
            suggestion.AppliedBy,
            suggestion.AppliedLeaseId
        ));
    }
}