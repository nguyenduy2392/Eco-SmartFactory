# =========================
# BUILD STAGE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# copy solution
COPY SmartFactory.sln .

# copy csproj từng project (tối ưu cache)
COPY SmartFactory.Application/SmartFactory.Application.csproj SmartFactory.Application/
COPY SmartFactory.Api/SmartFactory.Api.csproj SmartFactory.Api/

# restore
RUN dotnet restore SmartFactory.Api/SmartFactory.Api.csproj --disable-parallel

# copy toàn bộ source
COPY . .

# publish SmartFactory.Api
WORKDIR /src/SmartFactory.Api
RUN dotnet publish -c Release -o /app/publish

# =========================
# RUNTIME STAGE
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "SmartFactory.Api.dll"]
