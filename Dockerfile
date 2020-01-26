FROM microsoft/dotnet:2.2-sdk AS builder
WORKDIR /source

COPY . .
RUN dotnet restore PowerBinder.sln
RUN dotnet publish ./PowerBinder.Api/PowerBinder.Api.csproj --output /app/ --configuration Release

FROM microsoft/dotnet:2.2-aspnetcore-runtime
WORKDIR /app
COPY --from=builder /app .
ENTRYPOINT ["dotnet", "PowerBinder.Api.dll"]
