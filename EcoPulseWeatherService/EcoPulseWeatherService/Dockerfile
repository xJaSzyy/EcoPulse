FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EcoPulseWeatherService/EcoPulseWeatherService.csproj", "EcoPulseWeatherService/"]
RUN dotnet restore "EcoPulseWeatherService/EcoPulseWeatherService.csproj"
COPY . .
WORKDIR "/src/EcoPulseWeatherService"
RUN dotnet build "./EcoPulseWeatherService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EcoPulseWeatherService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EcoPulseWeatherService.dll"]
