FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["backend/FinTrack.Domain/FinTrack.Domain.csproj", "backend/FinTrack.Domain/"]
COPY ["backend/FinTrack.Application/FinTrack.Application.csproj", "backend/FinTrack.Application/"]
COPY ["backend/FinTrack.Infrastructure/FinTrack.Infrastructure.csproj", "backend/FinTrack.Infrastructure/"]
COPY ["backend/FinTrack.Api/FinTrack.Api.csproj", "backend/FinTrack.Api/"]

RUN dotnet restore "backend/FinTrack.Api/FinTrack.Api.csproj"

COPY . .

RUN dotnet publish "backend/FinTrack.Api/FinTrack.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "FinTrack.Api.dll"]