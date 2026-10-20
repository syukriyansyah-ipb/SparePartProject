# ============================================================
# STAGE 1: BUILD
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Sesuaikan nama file csproj dengan nama project baru Anda
COPY ["AutoPartsApi.csproj", "./"]
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ============================================================
# STAGE 2: RUNTIME
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Sesuaikan nama dll dengan nama project baru Anda
ENTRYPOINT ["dotnet", "AutoPartsApi.dll"]