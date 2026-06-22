# Base image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get update && \
  apt-get install -y tzdata apt-utils libgdiplus libc6-dev && \
  ln -s /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll && \
  rm -rf /var/lib/apt/lists/*

WORKDIR /app
EXPOSE 8080
ENV TZ=America/Costa_Rica
ENV ASPNETCORE_URLS=http://+:8080

# Build image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Dhole.Auth.Api/Dhole.Auth.Api.csproj", "src/Dhole.Auth.Api/"]
COPY ["src/Dhole.Auth.Application/Dhole.Auth.Application.csproj", "src/Dhole.Auth.Application/"]
COPY ["src/Dhole.Auth.Domain/Dhole.Auth.Domain.csproj", "src/Dhole.Auth.Domain/"]
COPY ["src/Dhole.Auth.Persistence/Dhole.Auth.Persistence.csproj", "src/Dhole.Auth.Persistence/"]
COPY ["src/Dhole.Auth.Infrastructure/Dhole.Auth.Infrastructure.csproj", "src/Dhole.Auth.Infrastructure/"]
COPY ["src/Dhole.Auth.Contracts/Dhole.Auth.Contracts.csproj", "src/Dhole.Auth.Contracts/"]

RUN dotnet restore "src/Dhole.Auth.Api/Dhole.Auth.Api.csproj"

COPY . .

WORKDIR "/src/src/Dhole.Auth.Api"
RUN dotnet build "Dhole.Auth.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

# Publish image
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/src/Dhole.Auth.Api

RUN dotnet publish "Dhole.Auth.Api.csproj" \
  -c $BUILD_CONFIGURATION \
  -o /app/publish \
  /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN chmod 755 /app

ENTRYPOINT ["dotnet", "Dhole.Auth.Api.dll"]
