# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Payroll.Api/Payroll.Api.csproj src/Payroll.Api/
COPY src/Payroll.Application/Payroll.Application.csproj src/Payroll.Application/
COPY src/Payroll.Domain/Payroll.Domain.csproj src/Payroll.Domain/
COPY src/Payroll.Infrastructure/Payroll.Infrastructure.csproj src/Payroll.Infrastructure/
RUN dotnet restore src/Payroll.Api/Payroll.Api.csproj

COPY src/ ./src/
RUN dotnet publish src/Payroll.Api/Payroll.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "Payroll.Api.dll"]
