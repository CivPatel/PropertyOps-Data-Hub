using Microsoft.EntityFrameworkCore;
using PropertyOps.Api.Data;
using PropertyOps.Api.Models;

namespace PropertyOps.Api.Services;

public class ApprovedCorrectionApplicationService
{
    private readonly PropertyOpsDbContext _db;

    public ApprovedCorrectionApplicationService(PropertyOpsDbContext db)
    {
        _db = db;
    }

    public async Task<CorrectionSuggestion> ApplyAsync(
        int suggestionId,
        string appliedBy,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(appliedBy))
        {
            throw new InvalidOperationException("AppliedBy is required.");
        }

        var suggestion = await _db.CorrectionSuggestions
            .Include(item => item.DataQualityError)
            .FirstOrDefaultAsync(
                item => item.Id == suggestionId,
                cancellationToken);

        if (suggestion is null)
        {
            throw new KeyNotFoundException(
                $"Correction suggestion with ID {suggestionId} was not found."
            );
        }

        if (suggestion.Status != "Approved")
        {
            throw new InvalidOperationException(
                $"Only Approved suggestions can be applied. Current status: {suggestion.Status}."
            );
        }

        if (!string.Equals(
                suggestion.FieldName,
                "propertyCode",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "This version can apply only propertyCode corrections."
            );
        }

        if (string.IsNullOrWhiteSpace(suggestion.SuggestedValue))
        {
            throw new InvalidOperationException(
                "The approved suggestion does not contain a corrected value."
            );
        }

        if (string.IsNullOrWhiteSpace(suggestion.DataQualityError?.RawData))
        {
            throw new InvalidOperationException(
                "The original rejected CSV row is unavailable."
            );
        }

        var values = CsvParser.ParseLine(suggestion.DataQualityError.RawData);

        if (values.Count < 10)
        {
            throw new InvalidOperationException(
                "The original row does not match the expected leasing CSV format."
            );
        }

        // CSV format:
        // externalLeaseId, propertyCode, unitNumber, residentName, monthlyRent,
        // status, leaseStartDate, leaseEndDate, moveInDate, moveOutDate

        values[1] = suggestion.SuggestedValue.Trim();

        var externalLeaseId = values[0].Trim();
        var propertyCode = values[1].Trim();
        var unitNumber = values[2].Trim();
        var residentName = values[3].Trim();
        var status = values[5].Trim();

        if (string.IsNullOrWhiteSpace(externalLeaseId) ||
            string.IsNullOrWhiteSpace(unitNumber) ||
            string.IsNullOrWhiteSpace(residentName) ||
            string.IsNullOrWhiteSpace(status))
        {
            throw new InvalidOperationException(
                "The corrected record is missing one or more required fields."
            );
        }

        if (!decimal.TryParse(values[4], out var monthlyRent) || monthlyRent < 0)
        {
            throw new InvalidOperationException(
                "The corrected record has an invalid monthly rent."
            );
        }

        if (!DateTime.TryParse(values[6], out var leaseStartDate) ||
            !DateTime.TryParse(values[7], out var leaseEndDate))
        {
            throw new InvalidOperationException(
                "The corrected record has invalid lease dates."
            );
        }

        DateTime? moveInDate = null;
        DateTime? moveOutDate = null;

        if (!string.IsNullOrWhiteSpace(values[8]))
        {
            if (!DateTime.TryParse(values[8], out var parsedMoveInDate))
            {
                throw new InvalidOperationException(
                    "The corrected record has an invalid move-in date."
                );
            }

            moveInDate = parsedMoveInDate;
        }

        if (!string.IsNullOrWhiteSpace(values[9]))
        {
            if (!DateTime.TryParse(values[9], out var parsedMoveOutDate))
            {
                throw new InvalidOperationException(
                    "The corrected record has an invalid move-out date."
                );
            }

            moveOutDate = parsedMoveOutDate;
        }

        if (leaseEndDate < leaseStartDate)
        {
            throw new InvalidOperationException(
                "Lease end date cannot be before lease start date."
            );
        }

        var property = await _db.Properties
            .FirstOrDefaultAsync(
                item => item.PropertyCode == propertyCode,
                cancellationToken);

        if (property is null)
        {
            throw new InvalidOperationException(
                $"Approved property code '{propertyCode}' no longer exists."
            );
        }

        var lease = await _db.Leases
            .FirstOrDefaultAsync(
                item => item.ExternalLeaseId == externalLeaseId,
                cancellationToken);

        if (lease is null)
        {
            lease = new Lease
            {
                ExternalLeaseId = externalLeaseId
            };

            _db.Leases.Add(lease);
        }

        lease.PropertyId = property.Id;
        lease.UnitNumber = unitNumber;
        lease.ResidentName = residentName;
        lease.MonthlyRent = monthlyRent;
        lease.Status = status;
        lease.LeaseStartDate = leaseStartDate;
        lease.LeaseEndDate = leaseEndDate;
        lease.MoveInDate = moveInDate;
        lease.MoveOutDate = moveOutDate;

        await _db.SaveChangesAsync(cancellationToken);

        suggestion.Status = "Applied";
        suggestion.AppliedAtUtc = DateTime.UtcNow;
        suggestion.AppliedBy = appliedBy.Trim();
        suggestion.AppliedLeaseId = lease.Id;

        await _db.SaveChangesAsync(cancellationToken);

        return suggestion;
    }
}