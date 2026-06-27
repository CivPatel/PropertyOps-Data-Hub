# PropertyOps Data Hub

A cloud-hosted backend and data-engineering platform that simulates how a multifamily real-estate company can consolidate leasing, construction, and maintenance data into a single operational source of truth.

Built with ASP.NET Core, SQL Server, Azure App Service, Azure SQL Database and Docker.

## Business Problem

Property-management organizations often receive data from multiple systems:

* Leasing platforms
* Construction and capital-project systems
* Maintenance/work-order systems
* Financial and operational reporting tools

PropertyOps Data Hub centralizes this information in SQL Server, validates incoming source data, logs rejected records, and exposes business reporting APIs for operational decision-making.

## Core Features

* CSV leasing-data ingestion pipeline
* Data validation and data-quality error logging
* Idempotent lease imports using external lease IDs
* Azure SQL database with EF Core migrations
* SQL Server stored procedures for reporting
* Portfolio occupancy and scheduled-rent reporting
* Construction budget-risk reporting
* Maintenance backlog and alert reporting
* API health monitoring endpoint
* Azure App Service deployment

## Architecture

```text
                    ┌──────────────────────┐
                    │ Leasing CSV Export   │
                    │ Construction Data    │
                    │ Maintenance Data     │
                    └──────────┬───────────┘
                               │
                               ▼
                  ┌──────────────────────────┐
                  │ ASP.NET Core Web API     │
                  │ PropertyOps Data Hub     │
                  └──────────┬───────────────┘
                             │
          ┌──────────────────┼──────────────────┐
          ▼                  ▼                  ▼
┌────────────────┐  ┌─────────────────┐  ┌──────────────────┐
│ Validation     │  │ Pipeline Logs   │  │ Data Quality     │
│ Required Fields│  │ Records Loaded  │  │ Error Records    │
│ Dates          │  │ Rejected Rows   │  │ Failed Fields    │
│ Property Codes │  │ Run Status      │  │ Raw Source Data  │
└────────────────┘  └─────────────────┘  └──────────────────┘
                             │
                             ▼
                  ┌──────────────────────────┐
                  │ Azure SQL Database       │
                  │ SQL Server Stored Procs  │
                  └──────────┬───────────────┘
                             │
                             ▼
                  ┌──────────────────────────┐
                  │ Dashboard / Reporting API│
                  │ Occupancy                │
                  │ Rent                     │
                  │ Construction Risk        │
                  │ Maintenance Alerts       │
                  └──────────────────────────┘
```

## Technology Stack

| Area              | Technologies                            |
| ----------------- | --------------------------------------- |
| Backend           | C#, ASP.NET Core Web API                |
| Database          | SQL Server, Azure SQL Database          |
| Data Pipeline     | CSV ingestion, validation, upsert logic |
| Cloud             | Azure App Service, Azure SQL            |
| API Documentation | Scalar / OpenAPI                        |

## Data Pipeline Design

The leasing ingestion pipeline follows this workflow:

```text
Extract → Validate → Transform → Load → Log → Monitor
```

### Validation Rules

The API validates:

* Required lease IDs
* Existing property codes
* Duplicate lease IDs in the uploaded file
* Positive monthly rent values
* Valid lease start and end dates
* Valid move-in and move-out dates
* Logical date order

Invalid source records are not silently discarded. They are stored in the `DataQualityErrors` table with:

* Source record ID
* Failed field
* Error message
* Raw source row
* Pipeline run ID
* Timestamp

## API Endpoints

### Health and Monitoring

| Method | Endpoint                   | Purpose                                |
| ------ | -------------------------- | -------------------------------------- |
| GET    | `/health`                  | Confirms API and database connectivity |
| GET    | `/api/pipeline-runs`       | Returns ingestion pipeline history     |
| GET    | `/api/data-quality-errors` | Returns rejected data records          |

### Leasing Data Ingestion

| Method | Endpoint                     | Purpose                                  |
| ------ | ---------------------------- | ---------------------------------------- |
| POST   | `/api/ingestion/leasing/csv` | Uploads and processes a leasing CSV file |

Example upload:

```bash
curl -X POST "https://propertyops-api-shiv-2026-a5d0azc4c2f5cwbw.centralus-01.azurewebsites.net/api/ingestion/leasing/csv" \
  -F "file=@src/PropertyOps.Api/SampleData/leasing-import.csv"
```

Example response:

```json
{
  "pipelineName": "Leasing CSV Import",
  "status": "CompletedWithErrors",
  "recordsReceived": 7,
  "recordsLoaded": 5,
  "recordsRejected": 2,
  "errorMessage": null
}
```

`CompletedWithErrors` is intentional in the sample file because it includes invalid source records to demonstrate data-quality validation.

### Dashboard and Reporting APIs

| Method | Endpoint                                             | Purpose                                                          |
| ------ | ---------------------------------------------------- | ---------------------------------------------------------------- |
| GET    | `/api/dashboard/portfolio-summary`                   | Portfolio occupancy, rent, maintenance, and construction summary |
| GET    | `/api/dashboard/properties/{propertyId}/performance` | Property-level performance metrics                               |
| GET    | `/api/dashboard/construction-risk`                   | Construction projects exceeding budget thresholds                |
| GET    | `/api/dashboard/maintenance-alerts`                  | Open and aging maintenance work orders                           |

## Local Setup

### Requirements

* .NET 10 SDK
* Docker Desktop
* Git
* SQL Server running through Docker Compose

## Screenshots

Add screenshots here after saving them inside a `docs/images` folder.

Suggested screenshots:

```text
docs/images/health-check.png
docs/images/pipeline-run.png
docs/images/data-quality-errors.png
docs/images/portfolio-summary.png
docs/images/construction-risk.png
```

## Interview Talking Points

> I built and deployed a cloud-hosted property operations data platform using ASP.NET Core, Azure App Service, and Azure SQL Database. The platform ingests leasing CSV data, validates business rules, upserts valid leases, logs rejected source records, and tracks pipeline history. I created SQL Server stored procedures for occupancy, rent, construction-risk, and maintenance reporting, then exposed those metrics through REST APIs. I also added Docker support, automated tests, GitHub Actions CI/CD, health monitoring, and retry handling for Azure SQL serverless database connections.

## Future Improvements

* React dashboard for business users
* Authentication and role-based authorization
* Application Insights monitoring
* Power BI reporting integration
