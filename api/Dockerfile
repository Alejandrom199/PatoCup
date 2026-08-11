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
# REGLA CRÍTICA: nunca hardcodear secretos acá. ENV queda visible con
# 'docker inspect' -- todo lo sensible se deja vacío y se inyecta en Railway
# como variable de entorno del servicio. ASP.NET Core mapea 'Seccion__Clave'
# a 'Seccion:Clave' de appsettings.json automáticamente.
ENV \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true \
    \
    # ── Base de datos (Supabase) — INYECTAR en Railway ────────────────────────
    ConnectionStrings__DefaultConnection="" \
    \
    # ── JWT — INYECTAR en Railway ─────────────────────────────────────────────
    Jwt__Key="" \
    Jwt__Issuer="PatoCupAPI" \
    Jwt__Audience="PatoCupFrontend" \
    \
    # ── CORS — URL del frontend en Netlify — INYECTAR en Railway ──────────────
    AppSettings__FrontendUrl=""

USER app

ENTRYPOINT ["dotnet", "PatoCup.WebAPI.dll"]
