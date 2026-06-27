using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PropertyOps.Api.Data;
using PropertyOps.Api.Dtos;
using PropertyOps.Api.Models;

namespace PropertyOps.Api.Services;

public class LeasingIngestionService
{
    private static readonly string[] RequiredColumns =
    {
        "externalLeaseId",
        "propertyCode",
        "unitNumber",
        "residentName",
        "monthlyRent",
        "status",
        "leaseStartDate",
        "leaseEndDate",
        "moveInDate",
        "moveOutDate"
    };

    private readonly PropertyOpsDbContext _db;
    private readonly ILogger<LeasingIngestionService> _logger;

    public LeasingIngestionService(
        PropertyOpsDbContext db,
        ILogger<LeasingIngestionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PipelineRunResponse> ImportAsync(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            throw new InvalidDataException("Please upload a non-empty CSV file.");
        }

        if (!Path.GetExtension(file.FileName)
            .Equals(".csv", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("Only CSV files are accepted.");
        }

        var pipelineRun = new PipelineRun
        {
            PipelineName = "Leasing CSV Import",
            Status = "Running",
            StartedAtUtc = DateTime.UtcNow
        };

        _db.PipelineRuns.Add(pipelineRun);
        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            var rows = await CsvParser.ReadAllRowsAsync(file);

            if (rows.Count < 2)
            {
                throw new InvalidDataException(
                    "The CSV needs one header row and at least one data row."
                );
            }

            var headerMap = BuildHeaderMap(rows[0]);
            ValidateRequiredColumns(headerMap);

            var executionStrategy = _db.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                var properties = (await _db.Properties
                    .AsNoTracking()
                    .ToListAsync(cancellationToken))
                    .ToDictionary(
                        property => property.PropertyCode,
                        StringComparer.OrdinalIgnoreCase
                    );

                var existingLeases = (await _db.Leases
                    .ToListAsync(cancellationToken))
                    .ToDictionary(
                        lease => lease.ExternalLeaseId,
                        StringComparer.OrdinalIgnoreCase
                    );

                var leaseIdsInThisFile = new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                );

                pipelineRun.RecordsReceived = 0;
                pipelineRun.RecordsLoaded = 0;
                pipelineRun.RecordsRejected = 0;

                await using var transaction =
                    await _db.Database.BeginTransactionAsync(cancellationToken);

                for (var rowIndex = 1; rowIndex < rows.Count; rowIndex++)
                {
                    var row = rows[rowIndex];
                    var sourceRowNumber = rowIndex + 1;

                    pipelineRun.RecordsReceived++;

                    var externalLeaseId = GetValue(
                        row, headerMap, "externalLeaseId");

                    var propertyCode = GetValue(
                        row, headerMap, "propertyCode");

                    var unitNumber = GetValue(
                        row, headerMap, "unitNumber");

                    var residentName = GetValue(
                        row, headerMap, "residentName");

                    var monthlyRentText = GetValue(
                        row, headerMap, "monthlyRent");

                    var status = GetValue(
                        row, headerMap, "status");

                    var leaseStartDateText = GetValue(
                        row, headerMap, "leaseStartDate");

                    var leaseEndDateText = GetValue(
                        row, headerMap, "leaseEndDate");

                    var moveInDateText = GetValue(
                        row, headerMap, "moveInDate");

                    var moveOutDateText = GetValue(
                        row, headerMap, "moveOutDate");

                    var validationErrors = new List<ImportValidationError>();

                    if (string.IsNullOrWhiteSpace(externalLeaseId))
                    {
                        validationErrors.Add(new(
                            "externalLeaseId",
                            "External lease ID is required."
                        ));
                    }
                    else if (!leaseIdsInThisFile.Add(externalLeaseId))
                    {
                        validationErrors.Add(new(
                            "externalLeaseId",
                            "Duplicate external lease ID found in this file."
                        ));
                    }

                    if (string.IsNullOrWhiteSpace(propertyCode))
                    {
                        validationErrors.Add(new(
                            "propertyCode",
                            "Property code is required."
                        ));
                    }
                    else if (!properties.TryGetValue(propertyCode, out _))
                    {
                        validationErrors.Add(new(
                            "propertyCode",
                            $"Property code '{propertyCode}' does not exist."
                        ));
                    }

                    if (string.IsNullOrWhiteSpace(unitNumber))
                    {
                        validationErrors.Add(new(
                            "unitNumber",
                            "Unit number is required."
                        ));
                    }

                    if (string.IsNullOrWhiteSpace(residentName))
                    {
                        validationErrors.Add(new(
                            "residentName",
                            "Resident name is required."
                        ));
                    }

                    if (string.IsNullOrWhiteSpace(status))
                    {
                        validationErrors.Add(new(
                            "status",
                            "Lease status is required."
                        ));
                    }

                    var monthlyRentIsValid = decimal.TryParse(
                        monthlyRentText,
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out var monthlyRent
                    );

                    if (!monthlyRentIsValid || monthlyRent < 0)
                    {
                        validationErrors.Add(new(
                            "monthlyRent",
                            "Monthly rent must be a non-negative number."
                        ));
                    }

                    var leaseStartDateIsValid = DateTime.TryParse(
                        leaseStartDateText,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var leaseStartDate
                    );

                    if (!leaseStartDateIsValid)
                    {
                        validationErrors.Add(new(
                            "leaseStartDate",
                            "Lease start date is invalid."
                        ));
                    }

                    var leaseEndDateIsValid = DateTime.TryParse(
                        leaseEndDateText,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var leaseEndDate
                    );

                    if (!leaseEndDateIsValid)
                    {
                        validationErrors.Add(new(
                            "leaseEndDate",
                            "Lease end date is invalid."
                        ));
                    }

                    DateTime? moveInDate = null;

                    if (!string.IsNullOrWhiteSpace(moveInDateText))
                    {
                        if (DateTime.TryParse(
                            moveInDateText,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out var parsedMoveInDate))
                        {
                            moveInDate = parsedMoveInDate;
                        }
                        else
                        {
                            validationErrors.Add(new(
                                "moveInDate",
                                "Move-in date is invalid."
                            ));
                        }
                    }

                    DateTime? moveOutDate = null;

                    if (!string.IsNullOrWhiteSpace(moveOutDateText))
                    {
                        if (DateTime.TryParse(
                            moveOutDateText,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out var parsedMoveOutDate))
                        {
                            moveOutDate = parsedMoveOutDate;
                        }
                        else
                        {
                            validationErrors.Add(new(
                                "moveOutDate",
                                "Move-out date is invalid."
                            ));
                        }
                    }

                    if (leaseStartDateIsValid &&
                        leaseEndDateIsValid &&
                        leaseEndDate < leaseStartDate)
                    {
                        validationErrors.Add(new(
                            "leaseEndDate",
                            "Lease end date cannot be before lease start date."
                        ));
                    }

                    if (moveInDate.HasValue &&
                        moveOutDate.HasValue &&
                        moveOutDate < moveInDate)
                    {
                        validationErrors.Add(new(
                            "moveOutDate",
                            "Move-out date cannot be before move-in date."
                        ));
                    }

                    if (validationErrors.Count > 0)
                    {
                        pipelineRun.RecordsRejected++;

                        foreach (var error in validationErrors)
                        {
                            _db.DataQualityErrors.Add(new DataQualityError
                            {
                                PipelineRunId = pipelineRun.Id,
                                SourceRecordId = string.IsNullOrWhiteSpace(externalLeaseId)
                                    ? $"CSV row {sourceRowNumber}"
                                    : externalLeaseId,
                                FieldName = error.FieldName,
                                ErrorMessage = error.Message,
                                RawData = string.Join(",", row),
                                CreatedAtUtc = DateTime.UtcNow
                            });
                        }

                        continue;
                    }

                    var property = properties[propertyCode];

                    if (existingLeases.TryGetValue(
                        externalLeaseId,
                        out var existingLease))
                    {
                        existingLease.PropertyId = property.Id;
                        existingLease.UnitNumber = unitNumber;
                        existingLease.ResidentName = residentName;
                        existingLease.MonthlyRent = monthlyRent;
                        existingLease.Status = status;
                        existingLease.LeaseStartDate = leaseStartDate;
                        existingLease.LeaseEndDate = leaseEndDate;
                        existingLease.MoveInDate = moveInDate;
                        existingLease.MoveOutDate = moveOutDate;
                    }
                    else
                    {
                        var newLease = new Lease
                        {
                            ExternalLeaseId = externalLeaseId,
                            PropertyId = property.Id,
                            UnitNumber = unitNumber,
                            ResidentName = residentName,
                            MonthlyRent = monthlyRent,
                            Status = status,
                            LeaseStartDate = leaseStartDate,
                            LeaseEndDate = leaseEndDate,
                            MoveInDate = moveInDate,
                            MoveOutDate = moveOutDate
                        };

                        _db.Leases.Add(newLease);
                        existingLeases[externalLeaseId] = newLease;
                    }

                    pipelineRun.RecordsLoaded++;
                }

                await _db.SaveChangesAsync(cancellationToken);

                pipelineRun.Status = pipelineRun.RecordsRejected > 0
                    ? "CompletedWithErrors"
                    : "Completed";

                pipelineRun.FinishedAtUtc = DateTime.UtcNow;

                await _db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return ToResponse(pipelineRun);
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Leasing import pipeline failed. PipelineRunId: {PipelineRunId}",
                pipelineRun.Id
            );

            _db.ChangeTracker.Clear();

            var failedRun = await _db.PipelineRuns.FindAsync(
                new object[] { pipelineRun.Id },
                cancellationToken
            );

            if (failedRun is not null)
            {
                failedRun.Status = "Failed";
                failedRun.FinishedAtUtc = DateTime.UtcNow;
                failedRun.ErrorMessage = exception.Message.Length > 1000
                    ? exception.Message[..1000]
                    : exception.Message;

                await _db.SaveChangesAsync(cancellationToken);
            }

            throw;
        }
    }

    private static Dictionary<string, int> BuildHeaderMap(string[] headers)
    {
        var headerMap = new Dictionary<string, int>(
            StringComparer.OrdinalIgnoreCase
        );

        for (var index = 0; index < headers.Length; index++)
        {
            var header = headers[index]
                .Trim()
                .TrimStart('\uFEFF');

            if (!string.IsNullOrWhiteSpace(header))
            {
                headerMap[header] = index;
            }
        }

        return headerMap;
    }

    private static void ValidateRequiredColumns(
        IReadOnlyDictionary<string, int> headerMap)
    {
        var missingColumns = RequiredColumns
            .Where(column => !headerMap.ContainsKey(column))
            .ToList();

        if (missingColumns.Count > 0)
        {
            throw new InvalidDataException(
                $"CSV is missing required columns: {string.Join(", ", missingColumns)}"
            );
        }
    }

    private static string GetValue(
        string[] row,
        IReadOnlyDictionary<string, int> headerMap,
        string columnName)
    {
        if (!headerMap.TryGetValue(columnName, out var columnIndex) ||
            columnIndex >= row.Length)
        {
            return string.Empty;
        }

        return row[columnIndex].Trim();
    }

    private static PipelineRunResponse ToResponse(PipelineRun run)
    {
        return new PipelineRunResponse(
            run.Id,
            run.PipelineName,
            run.Status,
            run.StartedAtUtc,
            run.FinishedAtUtc,
            run.RecordsReceived,
            run.RecordsLoaded,
            run.RecordsRejected,
            run.ErrorMessage
        );
    }

    private sealed record ImportValidationError(
        string FieldName,
        string Message
    );
}