# Use the official .NET SDK as the base image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY . .
RUN dotnet restore ./apps/backend/discord.bot/discord.bot.csproj
# RUN dotnet restore /src/apps/backend/discord.bot/discord.bot.csproj
# Build the project

RUN dotnet publish /src/apps/backend/discord.bot -c Release -o /app/out 

# Use the runtime-only image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Set the entrypoint for the container
ENTRYPOINT ["dotnet", "discord.bot.dll"]