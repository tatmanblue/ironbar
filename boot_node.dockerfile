# Set the base image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# Set the working directory
WORKDIR /node_dir

# Copy all project files and restore as distinct layers
COPY ./src ./src

WORKDIR /node_dir/src
RUN dotnet restore
RUN dotnet publish -c Release -o /node_dir/bin

# Expose the API port
EXPOSE 443
EXPOSE 80
EXPOSE 50051 

# Set the entry point for the API
WORKDIR /node_dir/bin
ENTRYPOINT ["dotnet", "node.dll"]

# in case of need
# ENTRYPOINT ["tail", "-f", "/dev/null"]