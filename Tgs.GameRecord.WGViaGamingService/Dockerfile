#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BuildConfiguration=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.GameRecord.WGViaGamingService/Tgs.GameRecord.WGViaGamingService.csproj", "Tgs.GameRecord.WGViaGamingService/"]
COPY ["Tgs.GameRecord.WGViaGamingService.Core/Tgs.GameRecord.WGViaGamingService.Core.csproj", "Tgs.GameRecord.WGViaGamingService.Core/"]
RUN dotnet restore "Tgs.GameRecord.WGViaGamingService/Tgs.GameRecord.WGViaGamingService.csproj"
COPY . .
WORKDIR "/src/Tgs.GameRecord.WGViaGamingService"
RUN dotnet build "Tgs.GameRecord.WGViaGamingService.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Tgs.GameRecord.WGViaGamingService.csproj" -c $BuildConfiguration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.GameRecord.WGViaGamingService.dll"]