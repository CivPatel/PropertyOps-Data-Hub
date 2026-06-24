namespace PropertyOps.Api.Dtos;

public record CreatePropertyRequest(
    string PropertyCode,
    string Name,
    string City,
    string State,
    int TotalUnits
);

public record PropertyResponse(
    int Id,
    string PropertyCode,
    string Name,
    string City,
    string State,
    int TotalUnits
);