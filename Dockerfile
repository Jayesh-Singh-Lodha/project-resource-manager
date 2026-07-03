# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project files for restore
COPY server/src/PRM.API/PRM.API.csproj server/src/PRM.API/
COPY server/src/PRM.Application/PRM.Application.csproj server/src/PRM.Application/
COPY server/src/PRM.Core/PRM.Core.csproj server/src/PRM.Core/
COPY server/src/PRM.Infrastructure/PRM.Infrastructure.csproj server/src/PRM.Infrastructure/
COPY server/src/PRM.Shared/PRM.Shared.csproj server/src/PRM.Shared/
COPY client/src/PRM.Console/PRM.Console.csproj client/src/PRM.Console/

# Restore dependencies
RUN dotnet restore server/src/PRM.API/PRM.API.csproj

# Copy the rest of the code
COPY . .

# Publish the API
WORKDIR /app/server/src/PRM.API
RUN dotnet publish -c Release -o /out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .

# Expose the port Render expects (defaults to 8080 for web services, ASP.NET Core 8 uses 8080 by default)
EXPOSE 8080

# Configure ASP.NET Core to use the SQLite DB and correct port
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the API
ENTRYPOINT ["dotnet", "PRM.API.dll"]
