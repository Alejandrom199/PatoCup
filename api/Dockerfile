# =============================================================================
# Stage 1 · base
# Runtime puro de ASP.NET 8. Sin SDK, sin compiladores, sin código fuente.
# =============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app

# Railway le asigna el puerto real vía la variable HTTP_PORTS / PORT en runtime;
# EXPOSE es solo documentación para quien lea la imagen.
EXPOSE 8080


# =============================================================================
# Stage 2 · build
# SDK completo: restore (capa cacheada) + publish en un solo stage.
# Esta imagen nunca llega a producción.
# =============================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# Copiar solo los .csproj primero: si el código cambia pero las dependencias no,
# Docker reutiliza la capa de 'dotnet restore' sin re-descargar NuGet.
COPY PatoCup.Domain/PatoCup.Domain.csproj                 PatoCup.Domain/
COPY PatoCup.Application/PatoCup.Application.csproj       PatoCup.Application/
COPY PatoCup.Infrastructure/PatoCup.Infrastructure.csproj PatoCup.Infrastructure/
COPY PatoCup.WebAPI/PatoCup.WebAPI.csproj                 PatoCup.WebAPI/

RUN dotnet restore "PatoCup.WebAPI/PatoCup.WebAPI.csproj"

COPY . .

RUN dotnet publish "PatoCup.WebAPI/PatoCup.WebAPI.csproj" \
        --configuration ${BUILD_CONFIGURATION} \
        --no-restore \
        --output /app/publish \
        /p:UseAppHost=false


# =============================================================================
# Stage 3 · final (la única imagen que corre en Railway)
# =============================================================================
FROM base AS final

WORKDIR /app

COPY --from=build --chown=app:app /app/publish .

# ── Variables de entorno de runtime ──────────────────────────────────────────
# Solo lo no sensible se declara acá (Docker linter marca como antipatrón usar
# ENV para nombres que suenan a secreto, aunque el valor esté vacío). Todo lo
# sensible (ConnectionStrings__DefaultConnection, Jwt__Key,
# AppSettings__FrontendUrl) se configura como variable de entorno del servicio
# directamente en el dashboard de Railway -- nunca en este archivo. ASP.NET
# Core mapea 'Seccion__Clave' a 'Seccion:Clave' de appsettings.json solo.
ENV \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

USER app

# Railway asigna el puerto público vía la variable de entorno PORT en runtime
# (no en build time, por eso no puede ir en ASPNETCORE_URLS como ENV fijo).
# Se usa /bin/sh -c + exec para poder expandir $PORT al arrancar, sin perder
# el beneficio de que dotnet sea PID 1 (exec reemplaza el proceso del shell
# en vez de crear un hijo, así que SIGTERM sigue llegando directo a .NET).
ENTRYPOINT ["/bin/sh", "-c", "exec dotnet PatoCup.WebAPI.dll --urls http://+:${PORT:-8080}"]
