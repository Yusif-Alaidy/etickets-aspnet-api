# ----------------------------
# STEP 1: Build the application
# ----------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the whole repo
COPY . .

# Move to the project folder
WORKDIR /src/etickets-aspnet-api

# Restore dependencies
RUN dotnet restore

# Build and publish the app
RUN dotnet publish -c Release -o /app/out

# ----------------------------
# STEP 2: Run the application
# ----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/out .

# Set ASP.NET Core to listen on port 8080
ENV ASPNETCORE_URLS=http://+:8080

# Expose the port
EXPOSE 8080

# Run the app
ENTRYPOINT ["dotnet", "etickets-aspnet-api.dll"]
