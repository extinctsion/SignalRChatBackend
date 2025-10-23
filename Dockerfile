# Create a stage for building the application.
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY ["ChatBackend.API/ChatBackend.csproj", "ChatBackend.API/"]
COPY ["ChatBackend.Models/ChatBackend.Models.csproj", "ChatBackend.Models/"]

RUN dotnet restore "ChatBackend.API/ChatBackend.csproj"

COPY . .

WORKDIR /src/ChatBackend.API

RUN dotnet build "ChatBackend.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "ChatBackend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

RUN apk add --no-cache curl

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ChatBackend.dll"]