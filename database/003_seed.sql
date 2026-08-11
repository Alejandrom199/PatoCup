-- ══════════════════════════════════════════════════════════════════════════
-- PatoCup — Seed de datos (convertido de "semilla de datos pato cup.sql")
--
-- Este script es idempotente: el TRUNCATE ... RESTART IDENTITY CASCADE del
-- inicio deja todas las tablas relevantes en cero antes de re-sembrar, para
-- poder correrlo varias veces en un entorno local/de pruebas sin duplicar
-- datos. Se puede correr tal cual sobre un Supabase recién creado (tablas
-- vacías -> el TRUNCATE no borra nada real).
--
-- BUG CORREGIDO respecto al script original: el INSERT de RolePermissions
-- asignaba los permisos a RoleId = 0 (que nunca existió), en vez del rol
-- Administrador real (Id = 1). Con ese bug, el usuario admin sembrado se
-- creaba SIN permisos. Aquí se asigna al rol 1 correctamente.
--
-- BUG CORREGIDO (2): el script original de SQL Server insertaba la contraseña
-- del admin en texto plano, pero AuthService/PasswordHasher.Verify espera un
-- hash BCrypt -- con eso el login del admin sembrado nunca hubiera funcionado.
-- Acá se inserta ya hasheada con BCrypt.Net-Next (mismo algoritmo que usa
-- PasswordHasher.cs) el password en texto plano sigue siendo "sysadmin123456".
-- ══════════════════════════════════════════════════════════════════════════

TRUNCATE TABLE
    security.role_permissions,
    security.option_actions,
    security.options,
    security.actions,
    security.menus,
    security.users,
    security.roles,
    competition.matches,
    competition.phases,
    competition.tournaments,
    competition.players,
    competition.match_status,
    competition.phase_status,
    competition.player_status,
    competition.tournament_status,
    security.general_status,
    security.global_catalogs
RESTART IDENTITY CASCADE;

-- ── Catálogos globales ───────────────────────────────────────────────────────
INSERT INTO security.global_catalogs (code, name, description)
VALUES
    ('GENE', 'Estados Generales', 'Disponibilidad lógica'),                          -- ID 1
    ('JUGA', 'Estados de Jugador', 'Flujo de aprobación para nuevos registros'),      -- ID 2
    ('TORN', 'Estados de Torneo', 'Ciclo de vida de un torneo'),                      -- ID 3
    ('FASO', 'Estados de Fase', 'Progreso de fases del torneo'),                      -- ID 4
    ('PART', 'Estados de Partida', 'Estado actual de enfrentamientos');               -- ID 5

-- ── Estados específicos por entidad ─────────────────────────────────────────
INSERT INTO security.general_status (id, global_catalog_id, code, name)
VALUES
    (1, 1, 'ACTV', 'Activo'),
    (2, 1, 'INAC', 'Inactivo'),
    (3, 1, 'SUSP', 'Suspendido');

INSERT INTO competition.player_status (id, global_catalog_id, code, name)
VALUES
    (1, 2, 'PEND', 'Pendiente'),
    (2, 2, 'ACCP', 'Aceptado'),
    (3, 2, 'REJC', 'Rechazado'),
    (4, 2, 'BANN', 'Baneado');

INSERT INTO competition.tournament_status (id, global_catalog_id, code, name)
VALUES
    (1, 3, 'DRFT', 'Borrador'),
    (2, 3, 'OPEN', 'Inscripciones Abiertas'),
    (3, 3, 'LIVE', 'En Curso'),
    (4, 3, 'FINI', 'Finalizado'),
    (5, 3, 'CANC', 'Cancelado');

INSERT INTO competition.phase_status (id, global_catalog_id, code, name)
VALUES
    (1, 4, 'SCHD', 'Programada'),
    (2, 4, 'LIVE', 'En Juego'),
    (3, 4, 'COMP', 'Completada');

INSERT INTO competition.match_status (id, global_catalog_id, code, name)
VALUES
    (1, 5, 'SCHD', 'Programada'),
    (2, 5, 'LIVE', 'En Juego'),
    (3, 5, 'COMP', 'Finalizada'),
    (4, 5, 'DISP', 'En Disputa');

-- ── Seguridad: menús, acciones, rol Administrador ───────────────────────────
INSERT INTO security.menus (name, icon, "order", state_id, created_at, is_deleted)
VALUES
    ('Seguridad', 'security', 1, 1, now(), false),
    ('Competencia', 'sports_esports', 2, 1, now(), false);

INSERT INTO security.actions (name, code, state_id, created_at, is_deleted)
VALUES
    ('Listar', 'LIST', 1, now(), false),
    ('Crear', 'CREATE', 1, now(), false),
    ('Actualizar', 'UPDATE', 1, now(), false),
    ('Eliminar', 'DELETE', 1, now(), false);

INSERT INTO security.roles (name, description, state_id)
VALUES ('Administrador', 'Acceso total a la plataforma PatoCup', 1);   -- Id = 1

-- ── Opciones de menú y permisos ─────────────────────────────────────────────
INSERT INTO security.options (menu_id, name, icon, route, state_id, created_at, is_deleted)
VALUES
    (1, 'Usuarios', 'people', '/security/users', 1, now(), false),
    (1, 'Roles', 'lock', '/security/roles', 1, now(), false),
    (1, 'Auditoria', 'history', '/audit', 1, now(), false),
    (2, 'Jugadores', 'sports_soccer', '/competition/players', 1, now(), false),
    (2, 'Torneos', 'emoji_events', '/competition/tournaments', 1, now(), false);

INSERT INTO security.option_actions (option_id, action_id, state_id, created_at, is_deleted)
SELECT o.id, a.id, 1, now(), false
FROM security.options o, security.actions a;

-- Asignar todos los permisos al rol Administrador (Id = 1)
INSERT INTO security.role_permissions (role_id, option_action_id, created_at, is_deleted)
SELECT 1, id, now(), false
FROM security.option_actions;

-- ── Usuario administrador ────────────────────────────────────────────────────
-- Hash BCrypt de "sysadmin123456" (work factor 11, igual que BCrypt.Net-Next.HashPassword por defecto)
SELECT * FROM security.fn_users_create(
    1,
    'admin',
    '$2a$11$mV8Zjc9hBxoB/mo9ZGspbOIiylZtV0/FwdnjkoSEdlSLwNK.9xKL6',
    'admin@patocup.com',
    'https://yt3.googleusercontent.com/EMyQXpzMLGeukWfGw_17yrUPd7jKzXU7VvoB9Zwr28hemM9VQLrpIEEz_mYMK3bFGZ2QL217mQ=s160-c-k-c0x00ffffff-no-rj'
);
