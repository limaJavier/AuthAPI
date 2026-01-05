# Stage 1: Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS publish
WORKDIR /AuthAPI

# Copy all projects
COPY src/AuthAPI.Api/AuthAPI.Api.csproj src/AuthAPI.Api/
COPY src/AuthAPI.Application/AuthAPI.Application.csproj src/AuthAPI.Application/
COPY src/AuthAPI.Domain/AuthAPI.Domain.csproj src/AuthAPI.Domain/
COPY src/AuthAPI.Infrastructure/AuthAPI.Infrastructure.csproj src/AuthAPI.Infrastructure/

# Create a new solution file
RUN dotnet new sln -n AuthAPI
# Add all projects to solution
RUN dotnet sln add src/**/*.csproj 

# Restore solution
RUN dotnet restore

# Copy the rest of the source code
COPY src/ src/

# Publish the app
RUN dotnet publish src/AuthAPI.Api/AuthAPI.Api.csproj \
    -c Release \
    -o /app/publish

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS run
WORKDIR /app

# Copy published output
COPY --from=publish /app/publish .

# Expose HTTP port
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "AuthAPI.Api.dll"]
