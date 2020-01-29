FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS builder
WORKDIR /source

COPY . .
RUN dotnet restore OpsBro.sln
RUN dotnet publish ./OpsBro.Api/OpsBro.Api.csproj --output /app/ --configuration Release

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine
WORKDIR /app
COPY --from=builder /app .
ENTRYPOINT ["dotnet", "OpsBro.Api.dll"]
