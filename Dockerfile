FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine3.18-amd64 AS build
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
    clang zlib1g-dev

WORKDIR /src
COPY ["dotnet-redfish.csproj", "."]
RUN dotnet restore "dotnet-redfish.csproj"
COPY . .
RUN dotnet build "./dotnet-redfish.csproj" -c Release -o /app/build
RUN dotnet publish "./dotnet-redfish.csproj" -c Release -o /app/publish /p:UseAppHost=true

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine3.18-amd64 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["./dotnet-redfish"]