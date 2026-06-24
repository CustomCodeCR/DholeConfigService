# Base image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get update && \
  apt-get install -y tzdata apt-utils libgdiplus libc6-dev && \
  ln -s /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll && \
  rm -rf /var/lib/apt/lists/*

WORKDIR /app
EXPOSE 8080
EXPOSE 8081
ENV TZ=America/Costa_Rica

# Build image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Dhole.Config.Api/Dhole.Config.Api.csproj", "src/Dhole.Config.Api/"]
COPY ["src/Dhole.Config.Application/Dhole.Config.Application.csproj", "src/Dhole.Config.Application/"]
COPY ["src/Dhole.Config.Domain/Dhole.Config.Domain.csproj", "src/Dhole.Config.Domain/"]
COPY ["src/Dhole.Config.Persistence/Dhole.Config.Persistence.csproj", "src/Dhole.Config.Persistence/"]
COPY ["src/Dhole.Config.Infrastructure/Dhole.Config.Infrastructure.csproj", "src/Dhole.Config.Infrastructure/"]
COPY ["src/Dhole.Config.Contracts/Dhole.Config.Contracts.csproj", "src/Dhole.Config.Contracts/"]
COPY ["src/Dhole.Config.Contracts/Protos/config_catalog.proto", "src/Dhole.Config.Contracts/Protos/"]

RUN dotnet restore "src/Dhole.Config.Api/Dhole.Config.Api.csproj"

COPY . .

WORKDIR "/src/src/Dhole.Config.Api"
RUN dotnet build "Dhole.Config.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

# Publish image
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/src/Dhole.Config.Api

RUN dotnet publish "Dhole.Config.Api.csproj" \
  -c $BUILD_CONFIGURATION \
  -o /app/publish \
  /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN chmod 755 /app

ENTRYPOINT ["dotnet", "Dhole.Config.Api.dll"]
