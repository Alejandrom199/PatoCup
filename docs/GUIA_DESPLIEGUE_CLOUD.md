# Guía general: desplegar una app full-stack con Supabase + Railway + Netlify

Documento unificado a partir de dos despliegues reales: **PatoCup** (.NET 8 +
Angular, migrado desde Azure/SQL Server) y el patrón ya probado en
**ProyectoSagradaFamilia**. No es específico de un stack — los conceptos de
Supabase/Railway/Netlify/CI-CD aplican igual con cualquier backend/frontend;
donde algo es específico de .NET/Angular se marca explícitamente.

Cada sección de "Errores reales" de este documento ocurrió de verdad durante
el despliegue de PatoCup — no son advertencias teóricas, son la lista real de
cosas que fallaron y por qué.

---

## 1. Arquitectura del patrón

```
┌─────────────┐        HTTPS/JSON        ┌─────────────┐       TCP/5432        ┌─────────────┐
│   Netlify   │ ───────────────────────► │   Railway   │ ─────────────────────►│   Supabase  │
│  (frontend  │ ◄─────────────────────── │  (backend,  │◄────────────────────  │ (Postgres + │
│  estático)  │      CORS + cookies      │  Docker)    │   connection pooler    │  extras)    │
└─────────────┘                          └─────────────┘                       └─────────────┘
      ▲                                        ▲                                     ▲
      │ build automático al                    │ build automático al                 │ solo se toca vía
      │ hacer push a GitHub                     │ hacer push a GitHub                 │ SQL Editor / MCP /
      │ (CI/CD)                                 │ (CI/CD)                             │ dashboard
```

- **Netlify** sirve el build estático del frontend (Angular/React/Vue/lo que sea). No corre servidor propio, no tiene backend.
- **Railway** corre el backend en un contenedor Docker, con IP/dominio público.
- **Supabase** es Postgres gestionado + extras (Auth, Storage, Realtime, API REST automática) — para este patrón solo se usa la base de datos, conectada directo desde el backend con una connection string tradicional (no vía el cliente `supabase-js`).
- Frontend y backend nunca comparten dominio → toda comunicación es **cross-origin** → CORS y cookies cross-site importan desde el día uno (sección 6).

---

## 2. CI/CD: qué pasa realmente al conectar un repo de GitHub

Tanto Railway como Netlify funcionan igual en este punto:

1. Al crear el servicio/sitio, autorizas la app de Railway/Netlify sobre tu cuenta de GitHub y eliges el repositorio.
2. La plataforma registra un **webhook** en el repo (`Settings → Webhooks` en GitHub, aunque normalmente no hace falta tocarlo a mano — la integración de GitHub App lo hace sola).
3. Cada `git push` a la rama configurada (normalmente `main`) dispara automáticamente: clonar el repo → correr el build → si sale bien, reemplazar el deployment activo por el nuevo.
4. Esto es "Continuous Deployment" — la build es la parte de "CI" (valida que compile/construya), el despliegue automático sin intervención humana es la "CD".

**Lo que NO es automático** (y causó fricción real en esta sesión):
- Cambiar una **variable de entorno** por sí solo, en algunas configuraciones de Railway, no siempre dispara un redeploy automático. Después de tocar variables, conviene disparar un **"Redeploy" manual** desde el dashboard para tener certeza de que se está usando el valor nuevo.
- El "Root Directory" (en Railway) o "Base directory" (en Netlify) — si el repo es un monorepo (backend y frontend en el mismo repo, como PatoCup con `api/` y `client/`), hay que decirle a cada plataforma explícitamente en qué subcarpeta vive lo que le corresponde. Si no, intenta buildear desde la raíz y falla o hace algo inesperado.

---

## 3. El error conceptual más caro de esta sesión: `.env.example` vs variables reales

Esto costó más tiempo que cualquier otra cosa, así que va primero.

**Dos cosas completamente distintas que se parecen:**

1. **Un `.env.example` en el repo** — es solo documentación para humanos: "estas son las credenciales que vas a necesitar conseguir". Nombres cortos y libres (`JWT_SECRET_KEY`, `SUPABASE_PASSWORD`...). **No tiene ninguna conexión automática con nada** a menos que exista además un `docker-compose.yml` (u otro mecanismo) que explícitamente traduzca esos nombres a los que la app realmente necesita, por ejemplo:
   ```yaml
   environment:
     JwtSettings__SecretKey: ${JWT_SECRET_KEY}   # <- acá ocurre la traducción
   ```

2. **Las variables de entorno reales que la plataforma (Railway) inyecta al proceso** — deben tener **exactamente** el nombre que el framework del backend espera. En .NET/ASP.NET Core, el patrón es `Seccion__Subseccion` (doble guion bajo), que se mapea automáticamente a `"Seccion": { "Subseccion": "valor" }` de `appsettings.json` — sin ningún paso de traducción intermedio, es un mecanismo *built-in* de `Microsoft.Extensions.Configuration.EnvironmentVariables`. Otros frameworks tienen su propio patrón (Node/dotenv suele usar el nombre plano tal cual, Django usa `os.environ`, etc.) — **hay que confirmar cuál usa tu framework específico**, no asumir que el `.env.example` sirve tal cual.

Si el repo **no tiene** ese paso traductor (docker-compose, etc.), como pasó en PatoCup, entonces `.env.example` es *solo* una checklist — las variables que realmente hay que pegar en Railway son otras, y hay que armarlas a mano.

**Regla práctica:** antes de pegar nada en el dashboard de la plataforma de hosting, confirma en el código del backend cómo lee la configuración (busca `GetSection`, `Environment.GetEnvironmentVariable`, `process.env`, etc.) y usa exactamente esos nombres.

---

## 4. Supabase (base de datos)

### 4.1 Crear el proyecto
Un proyecto Supabase = una instancia Postgres gestionada + servicios extra (Auth, Storage, Realtime, API REST automática vía PostgREST). Para un backend que se conecta directo por connection string (no usa `supabase-js`), solo importa la base de datos.

### 4.2 La contraseña de la base de datos
- Si el proyecto se crea vía la API de gestión (por ejemplo, un agente/script automatizado usándola), Supabase genera una contraseña aleatoria que **nunca se expone después** — ni por API ni reingresando al dashboard. Si no la guardaste en el momento exacto de la creación, hay que **regenerarla**: `Settings → Database → Database password → "Generate a new password"` (o "Reset database password" según la versión de la UI). Se muestra **una sola vez** — cópiala ahí mismo.
- **No confundir con las "API Keys"** (`sb_secret_...`, `sb_publishable_...` o el legacy `anon`/`service_role`). Esas son para el cliente `supabase-js`/PostgREST/Auth — un backend que conecta directo por TCP a Postgres nunca las usa. Viven en una sección distinta del dashboard.

### 4.3 Direct connection vs Session pooler vs Transaction pooler
Supabase ofrece tres formas de conectar, visibles en `Settings → Database → Connection string` con un selector:

| Modo | Puerto típico | Cuándo usar |
|---|---|---|
| **Direct connection** | 5432 | Conexión directa a Postgres. En muchos proyectos nuevos **solo es alcanzable por IPv6** — falla en silencio (timeout) desde plataformas sin salida IPv6 saliente. |
| **Session pooler** | 5432 (en el host del pooler, no el de direct connection) | Pooler de PgBouncer en modo *session* — cada conexión de la app mantiene una sesión Postgres dedicada mientras dura. Soporta IPv4. |
| **Transaction pooler** | 6543 | Pooler de PgBouncer en modo *transacción* — las conexiones se multiplexan agresivamente entre transacciones cortas. Es el que Supabase recomienda por defecto para apps serverless/muchas conexiones cortas. |

**Errores reales encontrados con esto en PatoCup:**
- Usar el host de **Direct connection** en vez del pooler → los requests nunca llegaban a responder (colgados).
- Usar el **Transaction pooler** (6543) desde Railway → las conexiones se quedaban colgadas indefinidamente sin ningún error, ni timeout explícito, ni ninguna traza del lado de Postgres (confirmado revisando los logs de Postgres de Supabase: cero intentos de conexión durante la ventana de fallos). Las mismas credenciales, mismo host, conectaban perfecto por `psql` desde una red normal.
- Cambiar al **Session pooler** (5432, mismo host que el Transaction pooler) resolvió el problema al instante.
- **No quedó 100% claro si es un problema puntual de la red de Railway en ese momento, o algo más general al combinar Railway + Transaction pooler.** Recomendación práctica: probar primero el Transaction pooler (es el oficialmente recomendado), y si hay timeouts de conexión sin ningún error visible del lado de la app, probar Session pooler antes de asumir que es un bug de código.

### 4.4 Formato de la connection string: ADO.NET vs URI
Supabase muestra por defecto el connection string en formato **URI** (estilo `psql`/Node/Python):
```
postgresql://postgres.<project_ref>:[YOUR-PASSWORD]@<host>:<puerto>/postgres
```
Pero muchos drivers (Npgsql para .NET, entre otros) esperan el formato **ADO.NET** (pares clave=valor):
```
Host=<host>;Port=<puerto>;Database=postgres;Username=postgres.<project_ref>;Password=<password>;SSL Mode=Require;Trust Server Certificate=true;
```
Son **incompatibles entre sí** — pegar el formato URI donde se espera ADO.NET (o viceversa) produce errores confusos, no un rechazo claro de "formato incorrecto". Confirma qué formato espera tu driver específico antes de pegar nada.

**Error real:** se copió literalmente el string de ejemplo del dashboard sin reemplazar el placeholder `[YOUR-PASSWORD]` — Postgres lo trató como password literal e inválido. Verifica que no quede ningún placeholder tipo `<...>` o `[...]` sin reemplazar antes de guardar.

### 4.5 Row Level Security (RLS)
Supabase marca como hallazgo de seguridad **crítico** que las tablas tengan RLS desactivado — por defecto, cualquier tabla es completamente accesible vía la API REST automática de PostgREST a quien tenga la `anon key`. Si tu patrón de acceso es "el backend conecta con la contraseña completa de Postgres, el frontend nunca toca Supabase directo", el riesgo real es menor (la anon key no sirve para nada si PostgREST no expone tus tablas), pero:
- Si tus tablas viven en el schema `public`, sí están expuestas por defecto vía la API REST.
- Poner las tablas en un schema propio (no `public`) que nunca se agregue a "exposed schemas" reduce el riesgo, pero segir la recomendación oficial de Supabase (activar RLS con políticas explícitas) es lo correcto a mediano plazo.

### 4.6 search_path en funciones PL/pgSQL
Si vas a usar funciones/procedimientos en vez de solo tablas: `ALTER DATABASE ... SET plpgsql.variable_conflict = ...` requiere privilegios de superusuario que el rol de gestión de Supabase **no tiene** (`permission denied to set parameter`). La alternativa que sí funciona es el pragma `#variable_conflict use_column` como primera línea del cuerpo de cada función. De forma similar, el linter de seguridad de Supabase recomienda `SET search_path = ''` explícito en cada función (evita "search path hijacking") — se agrega en la definición de la función (`... LANGUAGE plpgsql SET search_path = '' AS $$ ...`), no a nivel de base de datos.

---

## 5. Railway (backend)

### 5.1 Conectar el repo
`New Project → Deploy from GitHub repo`. Si es un monorepo, configurar **Root Directory** (ej. `api/` si el backend vive en una subcarpeta) — Railway busca el `Dockerfile`/`railway.toml` relativo a esa carpeta.

`railway.toml` (en la raíz del root directory configurado):
```toml
[build]
dockerfilePath = "Dockerfile"

[deploy]
healthcheckPath = "/health"
healthcheckTimeout = 600
restartPolicyType = "ON_FAILURE"
restartPolicyMaxRetries = 5
```
Esto requiere que la app tenga un endpoint `/health` que responda 200 sin depender de nada externo (ni base de datos, ni auth) — si depende de la base de datos y esta tiene problemas, el healthcheck fallará en cascada y Railway nunca marcará el servicio como saludable.

### 5.2 El puerto es dinámico — error real más difícil de diagnosticar de esta sesión
Railway asigna el puerto público vía la variable de entorno **`PORT`**, inyectada en **runtime**, no en build time. Un `Dockerfile` que hardcodea el puerto (ej. `ENV ASPNETCORE_URLS=http://+:8080` fijo, o simplemente confía en el puerto por defecto del framework) hace que el proceso escuche en un puerto que puede no coincidir con el que Railway está healthcheckeando.

**Síntoma real:** el build terminaba bien, la imagen se subía bien, pero el healthcheck fallaba indefinidamente con "service unavailable" en cada intento durante los 10 minutos de retry window, y `curl` directo a la URL pública devolvía 404 (el edge/proxy de Railway nunca enruta tráfico a un servicio que no pasó el healthcheck).

**Fix (para contenedores .NET, pero el concepto aplica a cualquier stack):** el `ENTRYPOINT` del Dockerfile necesita **leer `$PORT` en tiempo de ejecución**, no en build time. Como el `ENTRYPOINT` en forma "exec" (`["cmd", "arg"]`) no permite expansión de variables de shell, hay que usar `/bin/sh -c` + `exec` (el `exec` es importante: reemplaza el proceso del shell en vez de crear un hijo, así la app sigue siendo PID 1 y recibe `SIGTERM` correctamente en vez de que el shell se quede colgado):
```dockerfile
ENTRYPOINT ["/bin/sh", "-c", "exec dotnet MiApp.dll --urls http://+:${PORT:-8080}"]
```
(El fallback `:-8080` es para poder correr el mismo contenedor localmente sin que Railway inyecte `PORT`.)

### 5.3 `.dockerignore` con patrones incompletos
Si el `.dockerignore` dice `bin/` y `obj/` (sin `**/` adelante) solo excluye esas carpetas en la **raíz exacta** del contexto de build, no en subcarpetas de cada proyecto. En un monorepo con múltiples proyectos (ej. `PatoCup.WebAPI/obj/`, `PatoCup.Infrastructure/obj/`...), eso deja que `COPY . .` copie los artefactos de compilación locales (generados en la máquina de desarrollo, con rutas/plataforma distintas) dentro de la imagen, chocando con el `restore`/`build` limpio que ya se hizo dentro del contenedor Linux — produce errores de build confusos (`MSB4018` en el caso de .NET). Usar siempre `**/bin/` y `**/obj/` (glob recursivo).

Mismo problema exacto se puede dar en `.gitignore` — si dice `api/bin/` en vez de `**/bin/`, solo cubre esa ruta literal y las subcarpetas de cada subproyecto terminan comiteadas a git igual.

### 5.4 Variables de entorno: cómo verificar que realmente se guardaron
Patrón que se repitió varias veces en esta sesión: el usuario escribe/pega el valor de una variable, pero **no confirma el cambio explícitamente** (Railway a veces requiere presionar Enter, hacer clic fuera del campo, o aceptar un banner tipo "N changes pending → Deploy"), y el valor viejo (o vacío) sigue activo. Síntoma típico: un error de "value not initialized" o "key length is zero" en el arranque de cada request, que apunta directo a una variable de configuración vacía.

**Checklist cuando algo depende de una variable recién cambiada:**
1. ¿El nombre es EXACTO? (mayúsculas, guiones bajos simples vs dobles — típicamente un solo carácter mal puesto rompe todo el mapeo).
2. ¿El cambio se confirmó explícitamente (no solo escrito en el campo)?
3. ¿Se disparó un redeploy DESPUÉS de guardar el cambio?
4. ¿El deployment activo actual es posterior a ese redeploy (revisar el timestamp/estado en el dashboard, no asumir)?

### 5.5 Ojo con las URLs que terminan en `/`
Cualquier variable de entorno que sea una URL usada para comparación exacta (típicamente CORS `AllowedOrigins`) es extremadamente sensible a una barra `/` final. Los navegadores **nunca** envían el header `Origin` con barra final — así que si guardaste `https://mi-sitio.netlify.app/` (con barra, muy fácil de hacer sin querer si copiaste la URL desde la barra de direcciones del navegador) pero el código compara contra ese valor exacto, CORS nunca va a matchear y el navegador bloqueará todas las peticiones con "No 'Access-Control-Allow-Origin' header is present" — un mensaje que no menciona la barra en absoluto, así que hay que saber buscarla específicamente. Verificar con un `curl -X OPTIONS` simulando el `Origin` real es la forma más rápida de confirmar/descartar esto sin depender del navegador.

---

## 6. CORS y cookies cross-site

Con frontend y backend en dominios distintos (Netlify vs Railway), dos cosas tienen que estar bien simultáneamente:

### 6.1 CORS del lado del backend
El backend debe declarar explícitamente qué orígenes puede aceptar, con credenciales habilitadas:
```csharp
services.AddCors(options => {
    options.AddPolicy("AllowFrontend", policy => {
        policy.WithOrigins("https://mi-sitio.netlify.app")  // exacto, sin barra final
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();   // necesario si se usan cookies
    });
});
```
`AllowCredentials()` **no se puede combinar** con un wildcard `*` de orígenes — hay que listar orígenes explícitos.

### 6.2 Cookies cross-site: `SameSite`
Si la autenticación usa una cookie (en vez de, por ejemplo, un token en `localStorage` que el frontend adjunta manualmente en cada request), el atributo `SameSite` de esa cookie importa mucho:

| Valor | Comportamiento |
|---|---|
| `Strict` | La cookie **nunca** se envía en peticiones cross-site. Con frontend y backend en dominios distintos, el login "funciona" (el navegador acepta la cookie en la respuesta), pero la sesión se pierde inmediatamente — ninguna petición futura vuelve a mandar esa cookie. |
| `Lax` | Se envía en navegación de nivel superior (ej. seguir un link), pero no en la mayoría de requests AJAX cross-site. |
| `None` | Se envía siempre, incluso cross-site. **Requiere `Secure=true`** (solo HTTPS) — es obligatorio por especificación, si falta, el navegador la descarta igual. |

**Para el patrón Netlify↔Railway, la única opción viable es `SameSite=None` + `Secure=true`.**

**Síntoma real si esto está mal:** el login responde 200 con el `Set-Cookie` correcto, pero cualquier request subsiguiente que dependa de esa cookie falla como si el usuario nunca se hubiera autenticado — muy fácil de confundir con un problema del backend cuando en realidad es puramente del atributo de la cookie.

### 6.3 El frontend también tiene que pedir explícitamente enviar credenciales
Del lado del cliente HTTP (fetch/axios/HttpClient), cada request que dependa de la cookie de sesión necesita `credentials: 'include'` (fetch) o `withCredentials: true` (axios/Angular HttpClient) — sin esto, el navegador ni siquiera intenta mandar la cookie, sin importar qué tan bien esté configurado el backend.

---

## 7. Netlify (frontend)

### 7.1 `netlify.toml`
```toml
[build]
  base    = "client"                              # subcarpeta si es monorepo
  command = "npm run build -- --configuration production"
  publish = "dist/nombre-del-proyecto/browser"     # carpeta exacta que genera el build

[[redirects]]
  from   = "/*"
  to     = "/index.html"
  status = 200
```
El bloque `[[redirects]]` es **obligatorio** para cualquier SPA con router del lado del cliente (Angular Router, React Router, etc.) — sin él, recargar la página en una ruta profunda (ej. `/auth/login`) da 404, porque Netlify busca un archivo físico en esa ruta en vez de servir siempre `index.html` y dejar que el router de JS resuelva la ruta.

### 7.2 La URL del backend normalmente NO es una variable de entorno de Netlify
A diferencia de backends que leen `process.env` en runtime, un frontend estático (Angular/React/Vue) típicamente **hornea** la URL del backend dentro del bundle de JavaScript en tiempo de **build** (ej. `environment.production.ts` en Angular, `.env.production` + `import.meta.env` en Vite). Eso significa:
- No hace falta ninguna variable de entorno en el dashboard de Netlify para esto (a menos que se configure explícitamente ese mecanismo).
- Pero si cambias la URL del backend en el código, **el sitio ya desplegado sigue con la URL vieja hasta el próximo build** — hay que confirmar cuándo fue el último build de Netlify relativo a cuándo se hizo el cambio y push.
- Se puede verificar directamente qué URL quedó compilada descargando los archivos `.js` servidos y buscando el dominio del backend con `grep`/`curl`, sin depender de inspeccionar en el navegador.

---

## 8. Checklist de troubleshooting (por síntoma)

| Síntoma | Causas más probables, en orden de probabilidad |
|---|---|
| Healthcheck de Railway falla indefinidamente, `curl` a la URL pública da 404 | Puerto hardcodeado vs `$PORT` dinámico (5.2) |
| `500` en cada request, incluso en endpoints sin lógica (`/health`) | Alguna variable de configuración crítica (JWT key, etc.) vacía — se inicializa en cada request si el middleware la usa globalmente |
| Request se cuelga indefinidamente sin responder nunca (no es un 500 rápido) | Problema de conectividad de red hacia la base de datos — probar otro modo de pooler (4.3) |
| El servicio funciona pero cero trazas de conexión aparecen en los logs de Postgres | La conexión nunca se está estableciendo — descartar la connection string en sí probándola por fuera (con `psql`/cliente nativo) antes de seguir revisando código |
| `500` solo en operaciones de escritura, las de lectura funcionan bien | Revisar lógica posterior a la escritura (ej. logging de auditoría) — la operación principal puede haberse completado igual, verificar directo en la base |
| CORS: "No 'Access-Control-Allow-Origin' header" | Origen no coincide exacto — sospechar primero de una barra `/` final en la variable de entorno del origen permitido (5.5) |
| Login responde 200 pero la sesión no persiste en requests siguientes | `SameSite=Strict` en la cookie con frontend/backend en dominios distintos (6.2) |
| Ninguna petición aparece en la pestaña Network al enviar un formulario (ni siquiera fallida) | No es un problema de red — revisar la consola de JavaScript por errores antes que nada |
| El seed/datos "no aparecen" en el dashboard de la base de datos | Casi siempre es estar mirando el schema equivocado en el explorador de tablas (ej. `public` en vez de un schema propio) — confirmar con una query directa en el SQL Editor antes de asumir que faltan datos |

---

## 9. Higiene de secretos (aplica a cualquier stack)

- `.env` real (con valores reales) siempre en `.gitignore` — solo `.env.example` con placeholders va al repo.
- Nunca hardcodear secretos en `Dockerfile` vía `ENV` con valores reales — quedan visibles con `docker inspect` en cualquier momento futuro, incluso después de rotar la credencial real. Dejar esas variables vacías en el Dockerfile (documentando que se inyectan en la plataforma de hosting) o no declararlas ahí en absoluto.
- Si una contraseña/secreto queda expuesto sin querer en cualquier medio no controlado (un chat, un log compartido, un ticket de soporte), regenerarlo apenas sea posible — no vale la pena confiar en que "nadie más lo vio".
- Revisar patrones de `.gitignore`/`.dockerignore` con `git check-ignore -v <archivo>` — un patrón que "se ve bien" a simple vista puede no cubrir subcarpetas si falta el `**/` correspondiente (pasó tres veces distintas en esta sesión con archivos distintos).
