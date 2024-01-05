#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.GameRecord.WGViaGaming/Tgs.GameRecord.WGViaGaming.csproj", "Tgs.GameRecord.WGViaGaming/"]
COPY ["Tgs.GameRecord.WGViaGaming.Common/Tgs.GameRecord.WGViaGaming.Common.csproj", "Tgs.GameRecord.WGViaGaming.Common/"]
COPY ["Tgs.GameRecord.WGViaGaming.Core/Tgs.GameRecord.WGViaGaming.Core.csproj", "Tgs.GameRecord.WGViaGaming.Core/"]
RUN dotnet restore "./Tgs.GameRecord.WGViaGaming/./Tgs.GameRecord.WGViaGaming.csproj"
COPY . .
WORKDIR "/src/Tgs.GameRecord.WGViaGaming"
RUN dotnet build "./Tgs.GameRecord.WGViaGaming.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Tgs.GameRecord.WGViaGaming.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.GameRecord.WGViaGaming.dll"]