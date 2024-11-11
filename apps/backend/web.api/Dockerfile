# Use the official .NET SDK as the base image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY . .
RUN dotnet restore ./apps/backend/web.api/web.api.csproj
# RUN dotnet restore /src/apps/backend/web.api/web.api.csproj
# Build the project

RUN dotnet publish /src/apps/backend/web.api -c Release -o /app/out 

# Use the runtime-only image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
ARG OTEL_VERSION=1.4.0
ADD https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation/releases/download/v${OTEL_VERSION}/otel-dotnet-auto-install.sh otel-dotnet-auto-install.sh
RUN apt-get update && apt-get install -y curl unzip && \
    OTEL_DOTNET_AUTO_HOME="/otel-dotnet-auto" sh otel-dotnet-auto-install.sh && \
    chmod +x /otel-dotnet-auto/instrument.sh
WORKDIR /app
COPY --from=build /app/out ./

ENV OTEL_DOTNET_AUTO_HOME="/otel-dotnet-auto"
ENV OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4318
ENV OTEL_SERVICE_NAME=webapi
ENV OTEL_RESOURCE_ATTRIBUTES=service.namespace=backend,deployment.environment=production,service.instance.id=ubuntu-4gb-nbg1-2,service.version=0.1.0
# Set the entrypoint for the container
ENTRYPOINT ["/otel-dotnet-auto/instrument.sh", "dotnet", "web.api.dll"]