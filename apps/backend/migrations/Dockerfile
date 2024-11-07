FROM mcr.microsoft.com/dotnet/sdk:8.0

# Set the working directory
WORKDIR /app

# Install EF tools
RUN dotnet tool install dotnet-ef --tool-path /usr/local/bin

# Restore dependencies
COPY . .
RUN dotnet restore ./apps/backend/data.access/data.access.csproj

# Build the project
RUN dotnet build ./apps/backend/data.access/data.access.csproj

# Set entrypoint
ENTRYPOINT ["dotnet", "ef", "database", "update", "--project", "./apps/backend/data.access/data.access.csproj"]
