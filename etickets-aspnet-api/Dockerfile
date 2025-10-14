# ----------------------------
# STEP 1: Build the application
# ----------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy everything and restore dependencies
COPY . .
RUN dotnet restore

# Publish the app to the 'out' folder
RUN dotnet publish -c Release -o /app/out

# ----------------------------
# STEP 2: Run the application
# ----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Expose the default ASP.NET port
EXPOSE 8080

# Run the app
ENTRYPOINT ["dotnet", "etickets-aspnet-api.dll"]
