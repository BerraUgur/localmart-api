# 1. Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["Basic/WebAPI/WebAPI.csproj", "Basic/WebAPI/"]
RUN dotnet restore "Basic/WebAPI/WebAPI.csproj"

# Copy all files
COPY . .

# Publish
WORKDIR "/src/Basic/WebAPI"
RUN dotnet publish -c Release -o /app/publish

# 2. Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Run the application
ENTRYPOINT ["dotnet", "WebAPI.dll"]
