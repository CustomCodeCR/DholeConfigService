# syntax=docker/dockerfile:1

# Base image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
# Keep APT bounded: a degraded Debian mirror must not exhaust the self-hosted runner.
# Use host networking in the deployment workflow so Docker's bridge DNS is not a dependency.
RUN set -eux; \
  export DEBIAN_FRONTEND=noninteractive; \
  timeout --signal=TERM --kill-after=10s 180s apt-get \
    -o Acquire::Retries=2 -o Acquire::ForceIPv4=true \
    -o Acquire::http::Timeout=20 -o Acquire::https::Timeout=20 update; \
  timeout --signal=TERM --kill-after=10s 300s apt-get \
    -o Acquire::Retries=2 -o Acquire::ForceIPv4=true \
    -o Acquire::http::Timeout=20 -o Acquire::https::Timeout=20 \
    install -y --no-install-recommends \
      tzdata apt-utils libgdiplus libc6-dev libgssapi-krb5-2; \
  ln -sf /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll; \
  rm -rf /var/lib/apt/lists/*

WORKDIR /app
ENV TZ=America/Costa_Rica

# Build image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG GITHUB_ACTOR=github-actions
WORKDIR /src

COPY ["NuGet.Config", "./NuGet.Config"]
COPY ["src/Dhole.Config.Workers/Dhole.Config.Workers.csproj", "src/Dhole.Config.Workers/"]
COPY ["src/Dhole.Config.Api/Dhole.Config.Api.csproj", "src/Dhole.Config.Api/"]
COPY ["src/Dhole.Config.Application/Dhole.Config.Application.csproj", "src/Dhole.Config.Application/"]
COPY ["src/Dhole.Config.Domain/Dhole.Config.Domain.csproj", "src/Dhole.Config.Domain/"]
COPY ["src/Dhole.Config.Persistence/Dhole.Config.Persistence.csproj", "src/Dhole.Config.Persistence/"]
COPY ["src/Dhole.Config.Infrastructure/Dhole.Config.Infrastructure.csproj", "src/Dhole.Config.Infrastructure/"]
COPY ["src/Dhole.Config.Contracts/Dhole.Config.Contracts.csproj", "src/Dhole.Config.Contracts/"]

RUN --mount=type=secret,id=github_token,required=true \
  export NuGetPackageSourceCredentials_github="Username=${GITHUB_ACTOR};Password=$(cat /run/secrets/github_token);ValidAuthenticationTypes=Basic" && \
  dotnet restore "src/Dhole.Config.Workers/Dhole.Config.Workers.csproj" --configfile /src/NuGet.Config

COPY . .

WORKDIR "/src/src/Dhole.Config.Workers"
RUN dotnet build "Dhole.Config.Workers.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

# Publish image
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/src/Dhole.Config.Workers

RUN dotnet publish "Dhole.Config.Workers.csproj" \
  -c $BUILD_CONFIGURATION \
  -o /app/publish \
  --no-restore \
  /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN chmod 755 /app

ENTRYPOINT ["dotnet", "Dhole.Config.Workers.dll"]
