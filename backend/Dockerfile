# Use the official .NET 10 SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the solution and project files
COPY ["BOC.slnx", "./"]
COPY ["BOC.Domain/BOC.Domain.csproj", "BOC.Domain/"]
COPY ["BOC.Application/BOC.Application.csproj", "BOC.Application/"]
COPY ["BOC.Infrastructure/BOC.Infrastructure.csproj", "BOC.Infrastructure/"]
COPY ["BOC.WebAPI/BOC.WebAPI.csproj", "BOC.WebAPI/"]

# Restore dependencies
RUN dotnet restore "BOC.WebAPI/BOC.WebAPI.csproj"

# Copy the remaining source code
COPY . .
WORKDIR "/src/BOC.WebAPI"

# Build and publish the application
RUN dotnet build "BOC.WebAPI.csproj" -c Release -o /app/build
RUN dotnet publish "BOC.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose ports
EXPOSE 8080
EXPOSE 8443

# Start the API
ENTRYPOINT ["dotnet", "BOC.WebAPI.dll"]
