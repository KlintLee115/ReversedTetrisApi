# Use the official .NET SDK image to build and publish the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /app

# Copy the project file and restore dependencies
COPY ReversedTetrisApi.csproj ./
RUN dotnet restore

# Copy the rest of the application files
COPY . ./

# Build and publish the application to a folder named "out"
RUN dotnet publish -c Release -o out

# Use the official .NET runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory
WORKDIR /app

# Copy the published application files from the build stage
COPY --from=build /app/out .

# Expose the port that your application will run on
EXPOSE 8080

# Set the entry point for the container to run the application
ENTRYPOINT ["dotnet", "ReversedTetrisApi.dll"]
