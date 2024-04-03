# Set the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS build-env

# Set the working directory
WORKDIR /app

# Copy all project files and restore as distinct layers
COPY ../src/* ./
RUN dotnet restore

# Copy everything else and build
# COPY src/ .
RUN dotnet publish -c Release -o /app

# Expose the API port
EXPOSE 443
EXPOSE 80

# Set the entry point for the API
ENTRYPOINT ["dotnet", "node.exe"]