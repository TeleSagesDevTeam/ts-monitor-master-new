# Use the .NET SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

RUN apt-get update && apt-get install -y unzip

COPY ["ContractIndexer.sln", "./"]
COPY ["appsettings.json", "./"]
COPY ["src/ContractIndexer.csproj", "src/"]
COPY ["src/", "src/"]
COPY ["pocketbase-csharp-sdk.zip", "./"]

RUN unzip pocketbase-csharp-sdk.zip -d ./ && rm pocketbase-csharp-sdk.zip

# Restore the NuGet packages
RUN dotnet restore "src/ContractIndexer.csproj"

# Publish the application to the /app directory
RUN dotnet publish "src/ContractIndexer.csproj" -c Release -o /app --no-restore

# Use the .NET runtime image to run the application
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
COPY --from=build /source/appsettings.json ./
ENTRYPOINT ["dotnet", "ContractIndexer.dll"]