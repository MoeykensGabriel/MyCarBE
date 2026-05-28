# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["MyCarBE.API/MyCarBE.API.csproj", "MyCarBE.API/"]
COPY ["MyCarBE.Application/MyCarBE.Application.csproj", "MyCarBE.Application/"]
COPY ["MyCarBE.Data/MyCarBE.Data.csproj", "MyCarBE.Data/"]
COPY ["MyCarBE.Domain/MyCarBE.Domain.csproj", "MyCarBE.Domain/"]

RUN dotnet restore "MyCarBE.API/MyCarBE.API.csproj"

COPY . .
RUN dotnet publish "MyCarBE.API/MyCarBE.API.csproj" -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

RUN mkdir -p /app/wwwroot/uploads

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "MyCarBE.API.dll"]
