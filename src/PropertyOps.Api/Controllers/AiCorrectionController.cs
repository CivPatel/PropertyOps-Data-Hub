using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using PropertyOps.Api.Services;

namespace PropertyOps.Api.Controllers;

[ApiController]
[Route("api/ai-corrections")]
public class AiCorrectionController : ControllerBase
{
    private readonly AiCorrectionService _aiCorrectionService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AiCorrectionController> _logger;

    public AiCorrectionController(
        AiCorrectionService aiCorrectionService,
        IConfiguration configuration,
        ILogger<AiCorrectionController> logger)
    {
        _aiCorrectionService = aiCorrectionService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("generate/{dataQualityErrorId:int}")]
    public async Task<IActionResult> Generate(
        int dataQualityErrorId,
        [FromHeader(Name = "X-AI-Review-Key")] string? aiReviewKey,
        CancellationToken cancellationToken)
    {
        if (!HasValidAiReviewKey(aiReviewKey))
        {
            return Unauthorized(new
            {
                message = "A valid X-AI-Review-Key header is required."
            });
        }

        try
        {
            var suggestion = await _aiCorrectionService
                .GenerateForDataQualityErrorAsync(
                    dataQualityErrorId,
                    cancellationToken
                );

            return Ok(new
            {
                suggestionId = suggestion.Id,
                suggestion.DataQualityErrorId,
                suggestion.OriginalValue,
                suggestion.SuggestedValue,
                suggestion.Confidence,
                suggestion.Reason,
                suggestion.Status
            });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new
            {
                message = exception.Message
            });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new
            {
                message = exception.Message
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "AI correction generation failed for DataQualityErrorId {DataQualityErrorId}.",
                dataQualityErrorId
            );

            return StatusCode(
                StatusCodes.Status502BadGateway,
                new
                {
                    message = "The AI correction service could not generate a suggestion."
                }
            );
        }
    }

    private bool HasValidAiReviewKey(string? providedKey)
    {
        var expectedKey = _configuration["AIReview:AdminKey"];

        if (string.IsNullOrWhiteSpace(expectedKey) ||
            string.IsNullOrWhiteSpace(providedKey))
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(providedKey),
            Encoding.UTF8.GetBytes(expectedKey)
        );
    }
}