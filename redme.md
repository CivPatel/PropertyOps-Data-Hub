STEP 1

CREATE THE PROJECT 
SETUP DOTNET FOLDER
SETUP API FOLDER

STEP 2

ADD SQL SERVER WITH DOCKER

RUN DATABASE USING 
docker compose up -d

TO CHECK 
docker ps


STEP 3

STORE CONNECTION STRING SAFELY 

dotnet user-secrets set "ConnectionStrings:PropertyOpsDatabase" "Server=localhost,1433;Database=PropertyOpsHubDb;User Id=sa;Password=PropertyOps!2026Secure;TrustServerCertificate=True;Encrypt=False"

This keeps your local database password out of GitHub.

STPE 4

Create folders
Data
Models
Dtos
Controllers

STEP 5

ADD DATABASE MODELS 

/MODELS

/Property.cs
/Lease.cs
/ConstructionProject.cs
/WorkOrders.cs
/Pipelinerun.cs
/DataQualityError.cs

ADD DATABASE CONTEXT

/Data/PropertyOpsDbContext.cs
Data/DbInitializer.cs

ADD THE FIRST API ENDPOINT

Dtos/PropertyDtos.cs


ADD CONTROLLER

Controllers/PropertiesController.cs


STEP 6

PROGRAM.CS 


STEP 7

dotnet ef migrations add InitialCreate 
dotnet ef database update

STEP 8

RUN
dotnet run --urls http://localhost:5079

CHECK BROWSER
http://localhost:5079/scalar/v1
http://localhost:5079/health

CHECK ENDPOINTS 
GET /health
GET /api/properties
GET /api/properties/1
POST /api/properties

STEP 9

dotnet build 