# Etapa 1: Build (SDK de .NET 8)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copiamos todos los archivos de la carpeta 'api' 
# Esto incluye todas las subcarpetas de tus proyectos (Domain, Application, etc.)
COPY . .

# 2. Restauramos las dependencias apuntando específicamente al proyecto WebAPI
# .NET se encargará de buscar las dependencias en las otras carpetas automáticamente.
RUN dotnet restore "PatoCup.WebAPI/PatoCup.WebAPI.csproj"

# 3. Publicamos el proyecto WebAPI
RUN dotnet publish "PatoCup.WebAPI/PatoCup.WebAPI.csproj" -c Release -o /app/publish

# Etapa 2: Runtime (Imagen ligera para ejecutar)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# 4. PUNTO CRÍTICO: El nombre de la DLL de salida
# Según tu 'find', el proyecto es PatoCup.WebAPI, así que la DLL es PatoCup.WebAPI.dll
ENTRYPOINT ["dotnet", "PatoCup.WebAPI.dll"]