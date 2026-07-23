# 1. Use the official ASP.NET Core runtime as the base environment
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# 2. Use the .NET SDK to build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ZaatMarket.csproj", "./"]
RUN dotnet restore "./ZaatMarket.csproj"
COPY . .
RUN dotnet build "ZaatMarket.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 3. Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ZaatMarket.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 4. Final production container
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ZaatMarket.dll"]