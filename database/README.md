# Base de datos de PatoCup (PostgreSQL)

Esta carpeta es la fuente de verdad versionada del schema de PatoCup. Reemplaza los
scripts sueltos de SQL Server que se perdieron junto con el servidor original — si
esto se vuelve a perder, la base se reconstruye corriendo estos archivos en orden.

## Archivos

1. `001_schema.sql` — schemas (`security`, `competition`, `audit`), tablas, llaves foráneas.
2. `002_functions_security.sql` — funciones PL/pgSQL de seguridad (usuarios, roles, permisos, menú).
3. `002_functions_competition.sql` — funciones de torneos, fases, partidas, jugadores.
4. `002_functions_audit.sql` — funciones de auditoría.
5. `003_seed.sql` — catálogos base, rol Administrador con todos los permisos, usuario `admin`.

## Cómo aplicarlos

### Contra un Postgres local (para probar antes de tocar Supabase)

```bash
psql "postgresql://postgres:postgres@localhost:5432/postgres" -f database/001_schema.sql
psql "postgresql://postgres:postgres@localhost:5432/postgres" -f database/002_functions_security.sql
psql "postgresql://postgres:postgres@localhost:5432/postgres" -f database/002_functions_competition.sql
psql "postgresql://postgres:postgres@localhost:5432/postgres" -f database/002_functions_audit.sql
psql "postgresql://postgres:postgres@localhost:5432/postgres" -f database/003_seed.sql
```

### Contra Supabase (producción)

Usar el **SQL Editor** del dashboard de Supabase (Project → SQL Editor → New query),
pegar el contenido de cada archivo en ese mismo orden y ejecutar. Es preferible a
pasar la contraseña real de la base por `psql` en una terminal compartida.

Si prefieres `psql` directo desde tu máquina, usa la cadena de conexión de
**Session pooler** o **Direct connection** que te da Supabase en
Settings → Database → Connection string (no la de Transaction pooler, que no
soporta todos los comandos DDL de este script).

## Notas de la conversión (SQL Server → PostgreSQL)

- Nombres: todo en snake_case minúsculas (`competition.tournaments`,
  `fn_tournaments_get_all`) — es la convención nativa de Postgres, evita tener
  que citar identificadores con comillas dobles en cada query.
- Las funciones de "acción" (create/update/soft-delete/reactivate) devuelven una
  fila con `error_code`/`error_message` (y `new_id` cuando aplica) en vez de usar
  parámetros `OUTPUT` como en SQL Server — así el código C# las llama con
  `QueryFirstOrDefaultAsync` en vez de `ExecuteAsync` + parámetros de salida.
- `IDENTITY(1,1)` → `GENERATED ALWAYS AS IDENTITY`, `TINYINT` → `SMALLINT`,
  `BIT` → `BOOLEAN`, `NVARCHAR(n)` → `VARCHAR(n)`, `DATETIME` → `TIMESTAMPTZ`,
  `GETDATE()`/`SYSDATETIME()` → `now()`.
- Las transacciones explícitas (`BEGIN TRANSACTION`/`COMMIT`/`ROLLBACK`) del SP
  `sp_Phases_Update` se eliminaron: una función PL/pgSQL ya es atómica dentro de
  la transacción que la invoca, así que no hace falta manejarlo a mano.
- Se corrigieron dos problemas detectados en el código/scripts originales:
  - `sp_Users_Reactivate` no existía en los scripts recuperados (aunque el
    código C# lo llamaba) — se reconstruyó siguiendo el mismo patrón que
    `fn_tournaments_reactivate`.
  - El seed original asignaba los permisos del rol Administrador a `RoleId = 0`
    (inexistente) en vez de `1` — con ese bug el usuario admin quedaba sin
    permisos. Corregido en `003_seed.sql`.
- El schema de Roles (`security.roles`, `role_permissions`, etc.) se migró
  completo aunque hoy no tiene repositorio/controlador C# que lo use — para no
  perder esa parte de la base, no porque esté conectado a la API todavía.

## Gotchas reales encontrados al probar contra Postgres (no evidentes solo leyendo el SQL)

Estos 4 se descubrieron corriendo la migración completa end-to-end contra un
Postgres local antes de tocar Supabase — vale la pena conocerlos si tocas estas
funciones más adelante:

1. **`OFFSET ... ROWS FETCH NEXT ... ROWS ONLY` con una expresión aritmética
   como offset da error de sintaxis en Postgres.** Se cambió toda la paginación
   a la sintaxis nativa `LIMIT n OFFSET m`, que sí acepta expresiones sin problema.

2. **`RETURNS TABLE(...)` declara esas columnas como variables OUT implícitas
   visibles en todo el cuerpo de la función.** Como muchas columnas de retorno
   se llaman igual que columnas reales de las tablas (`id`, `username`,
   `state_id`...), cualquier referencia sin calificar como
   `WHERE username = p_username` queda ambigua. La solución "de una sola vez"
   sería `ALTER DATABASE postgres SET plpgsql.variable_conflict = use_column;`,
   pero en Supabase managed el rol de la API de gestión no tiene privilegios
   de superusuario para eso (`permission denied to set parameter`). En su
   lugar, cada función `LANGUAGE plpgsql` arranca con el pragma
   `#variable_conflict use_column` como primera línea del cuerpo — no requiere
   privilegios especiales y tiene el mismo efecto, solo que hay que
   acordarse de ponerlo en cada función nueva.

3. **`CommandType.StoredProcedure` de Npgsql llama a la función con notación
   de parámetros con nombre** (`fn(nombre_param => valor)`), usando como
   nombre el que le dio Dapper (`@Username` → `Username`). Como las funciones
   usan parámetros prefijados `p_` (para evitar el problema del punto 2 a nivel
   de nombres), esa llamada nunca encuentra la función. La solución en los
   repositorios C# es usar `CommandType.Text` con una llamada posicional
   explícita: `"SELECT * FROM schema.fn_x(@Param1, @Param2)"` — así Postgres
   recibe los valores por posición, sin que importe cómo se llamen los
   parámetros internos de la función ni los placeholders de Dapper.

4. **Los parámetros de función declarados `SMALLINT` no resuelven cuando C#
   envía un `int`.** Npgsql manda los `int` de C# como `integer` (int4), y
   Postgres no considera válido el cast implícito integer→smallint al resolver
   qué función llamar (sí es válido el cast al revés). Todos los parámetros de
   entrada que reciben un `StateId`/`PhaseStateId` desde C# quedaron como
   `INT` en vez de `SMALLINT` — las *columnas* de las tablas sí se mantienen en
   `SMALLINT`, el problema es solo en la firma de la función.

Además, la contraseña del usuario `admin` del seed va hasheada con BCrypt (ver
nota en `003_seed.sql`) — el script original de SQL Server la insertaba en
texto plano, lo que habría roto el login contra `PasswordHasher.Verify`.

## Estado en Supabase

Este schema ya está desplegado y verificado en el proyecto real `PatoCupDb`
(Supabase, región `us-east-2`). Todas las funciones tienen `SET search_path = ''`
(recomendación del linter de seguridad de Supabase — todas las referencias a
tablas ya van completamente calificadas con schema, así que no rompe nada).

**Pendiente de decisión del usuario:** Row Level Security está desactivado en
las 18 tablas. El riesgo real es bajo porque el backend .NET se conecta con la
contraseña completa de Postgres (no con la `anon key` pública) y las tablas no
están en el schema `public` expuesto por la API REST automática de Supabase —
pero sigue siendo la recomendación estándar de Supabase activarlo. El SQL de
remediación está disponible si se decide aplicarlo más adelante.

## Railway: usar el Session pooler, no el Transaction pooler

Verificado en despliegue real: desde Railway, el **Transaction pooler**
(puerto `6543`) de Supabase se queda colgado indefinidamente al conectar —
sin error, sin timeout explícito, sin ninguna traza en los logs de Postgres
de Supabase (la conexión nunca llega a establecerse). El mismo host en el
puerto del **Session pooler** (`5432`) conecta al instante. Verificado
también que las credenciales y el host en sí eran correctos (`psql` desde
una red normal conecta sin problemas a ambos puertos) — el problema es
específico de la ruta de red entre Railway y el Transaction pooler.

No quedó claro si es un problema puntual de esa red de Railway en ese
momento o algo más general del Transaction pooler visto desde plataformas
tipo Railway. Si vuelves a montar esto (u otra plataforma similar) y ves
timeouts de conexión sin ningún error del lado de la app, prueba cambiar de
Transaction pooler a Session pooler antes de asumir un bug de código.
