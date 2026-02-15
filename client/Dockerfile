# Etapa 1: Construcción (Build)
FROM node:20-alpine AS build-step
WORKDIR /app

# 1. Copiamos los archivos de dependencias desde la carpeta client
# Como el contexto es la raíz, debemos especificar la ruta 'client/'
COPY client/package*.json ./

# 2. Instalamos las dependencias de Node
RUN npm install

# 3. Copiamos todo el código fuente del frontend
COPY client/ .

# 4. Generamos el build de producción
# Esto creará la carpeta /app/dist/
RUN npm run build --configuration=production

# Etapa 2: Servidor Web (Nginx)
FROM nginx:alpine

# 5. Limpiamos la configuración y archivos por defecto de Nginx
RUN rm -rf /usr/share/nginx/html/* && rm -rf /etc/nginx/conf.d/*

# 6. Copiamos el resultado del build al servidor Nginx
# Usamos el comodín '*' para que Docker encuentre la carpeta del proyecto 
# sin importar si se llama 'client', 'pato-cup' o 'pato-cup-front'.
# En Angular 18, la ruta estándar es dist/[proyecto]/browser
COPY --from=build-step /app/dist/*/browser /usr/share/nginx/html

# 7. Copiamos tu configuración personalizada de Nginx
# Esta se encuentra en la raíz del monorepo
COPY nginx.conf /etc/nginx/conf.d/default.conf

# 8. Exponemos el puerto y arrancamos Nginx
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]