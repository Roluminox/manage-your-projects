# ===========================================
# Stage 1: Build
# ===========================================
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy solution and project files for restore
COPY MYP.sln ./
COPY src/MYP.Domain/MYP.Domain.csproj ./src/MYP.Domain/
COPY src/MYP.Application/MYP.Application.csproj ./src/MYP.Application/
COPY src/MYP.Infrastructure/MYP.Infrastructure.csproj ./src/MYP.Infrastructure/
COPY src/MYP.API/MYP.API.csproj ./src/MYP.API/

# Restore dependencies
RUN dotnet restore src/MYP.API/MYP.API.csproj

# Copy source code
COPY src/ ./src/

# Build and publish
RUN dotnet publish src/MYP.API/MYP.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ===========================================
# Stage 2: Runtime
# ===========================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

# Create non-root user for security
RUN addgroup -S appgroup && adduser -S appuser -G appgroup

# Copy published app
COPY --from=build /app/publish .

# Set ownership
RUN chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 5000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:5000/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Entry point
ENTRYPOINT ["dotnet", "MYP.API.dll"]
