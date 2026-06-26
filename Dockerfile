FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/PropertyOps.Api/PropertyOps.Api.csproj", "src/PropertyOps.Api/"]
RUN dotnet restore "src/PropertyOps.Api/PropertyOps.Api.csproj"

COPY . .
WORKDIR "/src/src/PropertyOps.Api"

RUN dotnet publish "PropertyOps.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PropertyOps.Api.dll"]