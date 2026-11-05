# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ProductManagement.Api/ProductManagement.Api.csproj ProductManagement.Api/
WORKDIR /src/ProductManagement.Api

RUN dotnet restore ProductManagement.Api.csproj

COPY ProductManagement.Api/. .

RUN dotnet publish ProductManagement.Api.csproj -c Release -o /app/publish

# Stage 2: Run the application
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ProductManagement.Api.dll"]
