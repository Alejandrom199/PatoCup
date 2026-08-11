-- ══════════════════════════════════════════════════════════════════════════
-- PatoCup — Schema PostgreSQL (Supabase)
-- Convertido desde los scripts originales de SQL Server recuperados en
-- pato-cup-base. Convenciones aplicadas:
--   - snake_case en minúsculas para schemas/tablas/columnas (nativo de
--     Postgres, evita tener que citar identificadores con comillas).
--   - IDENTITY(1,1)      -> GENERATED ALWAYS AS IDENTITY
--   - TINYINT            -> SMALLINT
--   - BIT                -> BOOLEAN
--   - NVARCHAR(n)        -> VARCHAR(n)
--   - NVARCHAR(MAX)      -> TEXT
--   - DATETIME           -> TIMESTAMPTZ
--   - GETDATE()/SYSDATETIME() -> now()
-- Orden de creación respeta las dependencias de FK.
-- ══════════════════════════════════════════════════════════════════════════

-- Las funciones usan RETURNS TABLE(...) con columnas que se llaman igual que
-- columnas reales de las tablas (id, username, name, state_id...). Postgres
-- declara esas columnas como variables OUT implícitas visibles en todo el
-- cuerpo de la función, así que una referencia sin calificar como
-- "WHERE username = p_username" queda ambigua entre la columna de la tabla y
-- esa variable OUT. Cada función LANGUAGE plpgsql en 002_functions_*.sql
-- arranca con el pragma "#variable_conflict use_column" para resolver esto
-- (NO se puede hacer a nivel de base de datos vía ALTER DATABASE en Supabase
-- managed: el rol de la API de gestión no tiene privilegios de superusuario
-- para eso, da "permission denied to set parameter").

CREATE SCHEMA IF NOT EXISTS security;
CREATE SCHEMA IF NOT EXISTS competition;
CREATE SCHEMA IF NOT EXISTS audit;

-- ─────────────────────────────────────────────────────────────────────────
-- Catálogos globales y estados
-- ─────────────────────────────────────────────────────────────────────────

CREATE TABLE security.global_catalogs (
    id          INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    code        VARCHAR(50) NOT NULL UNIQUE,
    name        VARCHAR(100) NOT NULL,
    description VARCHAR(200),
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE security.general_status (
    id                 SMALLINT PRIMARY KEY,
    global_catalog_id  INT NOT NULL REFERENCES security.global_catalogs(id),
    code               CHAR(4) NOT NULL UNIQUE,
    name               VARCHAR(50) NOT NULL,
    description        VARCHAR(250),
    created_at         TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE competition.tournament_status (
    id                 SMALLINT PRIMARY KEY,
    global_catalog_id  INT NOT NULL REFERENCES security.global_catalogs(id),
    code               CHAR(4) NOT NULL UNIQUE,
    name               VARCHAR(50) NOT NULL,
    description        VARCHAR(250),
    created_at         TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE competition.player_status (
    id                 SMALLINT PRIMARY KEY,
    global_catalog_id  INT NOT NULL REFERENCES security.global_catalogs(id),
    code               CHAR(4) NOT NULL UNIQUE,
    name               VARCHAR(50) NOT NULL,
    description        VARCHAR(250),
    created_at         TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE competition.phase_status (
    id                 SMALLINT PRIMARY KEY,
    global_catalog_id  INT NOT NULL REFERENCES security.global_catalogs(id),
    code               CHAR(4) NOT NULL UNIQUE,
    name               VARCHAR(50) NOT NULL,
    created_at         TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE competition.match_status (
    id                 SMALLINT PRIMARY KEY,
    global_catalog_id  INT NOT NULL REFERENCES security.global_catalogs(id),
    code               CHAR(4) NOT NULL UNIQUE,
    name               VARCHAR(50) NOT NULL,
    created_at         TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- ─────────────────────────────────────────────────────────────────────────
-- Seguridad: roles, usuarios, menús/opciones/acciones/permisos
-- ─────────────────────────────────────────────────────────────────────────

CREATE TABLE security.roles (
    id          INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name        VARCHAR(50) NOT NULL,
    description VARCHAR(200),
    state_id    SMALLINT NOT NULL REFERENCES security.general_status(id),
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at  TIMESTAMPTZ,
    is_deleted  BOOLEAN NOT NULL DEFAULT false,
    deleted_at  TIMESTAMPTZ
);

CREATE TABLE security.users (
    id          INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    role_id     INT NOT NULL REFERENCES security.roles(id),
    username    VARCHAR(50) NOT NULL UNIQUE,
    password    VARCHAR(255) NOT NULL,
    email       VARCHAR(100) NOT NULL,
    photo_url   VARCHAR(500),
    state_id    SMALLINT NOT NULL REFERENCES security.general_status(id),
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at  TIMESTAMPTZ,
    is_deleted  BOOLEAN NOT NULL DEFAULT false,
    deleted_at  TIMESTAMPTZ
);

CREATE TABLE security.menus (
    id          INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name        VARCHAR(50) NOT NULL,
    icon        VARCHAR(50),
    "order"     INT NOT NULL,
    state_id    SMALLINT NOT NULL REFERENCES security.general_status(id),
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now(),
    is_deleted  BOOLEAN NOT NULL DEFAULT false
);

CREATE TABLE security.options (
    id          INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    menu_id     INT NOT NULL REFERENCES security.menus(id),
    name        VARCHAR(50) NOT NULL,
    icon        VARCHAR(50),
    route       VARCHAR(100),
    state_id    SMALLINT NOT NULL REFERENCES security.general_status(id),
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now(),
    is_deleted  BOOLEAN NOT NULL DEFAULT false
);

CREATE TABLE security.actions (
    id          INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name        VARCHAR(50) NOT NULL,
    code        VARCHAR(50) NOT NULL UNIQUE,
    state_id    SMALLINT NOT NULL REFERENCES security.general_status(id),
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now(),
    is_deleted  BOOLEAN NOT NULL DEFAULT false
);

CREATE TABLE security.option_actions (
    id          INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    option_id   INT NOT NULL REFERENCES security.options(id),
    action_id   INT NOT NULL REFERENCES security.actions(id),
    state_id    SMALLINT NOT NULL REFERENCES security.general_status(id),
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now(),
    is_deleted  BOOLEAN NOT NULL DEFAULT false
);

CREATE TABLE security.role_permissions (
    id               INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    role_id          INT NOT NULL REFERENCES security.roles(id),
    option_action_id INT NOT NULL REFERENCES security.option_actions(id),
    created_at       TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at       TIMESTAMPTZ,
    deleted_at       TIMESTAMPTZ,
    is_deleted       BOOLEAN NOT NULL DEFAULT false
);

-- ─────────────────────────────────────────────────────────────────────────
-- Competencia: jugadores, torneos, fases, partidas
-- ─────────────────────────────────────────────────────────────────────────

CREATE TABLE competition.players (
    id                INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    nickname          VARCHAR(50) NOT NULL,
    game_id           VARCHAR(20) NOT NULL,
    registration_ip   VARCHAR(45) NOT NULL,
    player_state_id   SMALLINT NOT NULL REFERENCES competition.player_status(id),
    state_id          SMALLINT NOT NULL REFERENCES security.general_status(id),
    created_at        TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at        TIMESTAMPTZ,
    deleted_at        TIMESTAMPTZ,
    is_deleted        BOOLEAN NOT NULL DEFAULT false
);

CREATE TABLE competition.tournaments (
    id                   INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name                 VARCHAR(100) NOT NULL,
    description          VARCHAR(500),
    start_date           TIMESTAMPTZ,
    end_date             TIMESTAMPTZ,
    tournament_state_id  SMALLINT NOT NULL REFERENCES competition.tournament_status(id),
    state_id             SMALLINT NOT NULL REFERENCES security.general_status(id),
    created_at           TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at           TIMESTAMPTZ,
    deleted_at           TIMESTAMPTZ,
    is_deleted           BOOLEAN NOT NULL DEFAULT false,
    is_public            BOOLEAN NOT NULL DEFAULT false
);

CREATE TABLE competition.phases (
    id              INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    tournament_id   INT NOT NULL REFERENCES competition.tournaments(id),
    name            VARCHAR(50) NOT NULL,
    state_id        SMALLINT NOT NULL REFERENCES security.general_status(id),
    phase_state_id  SMALLINT NOT NULL REFERENCES competition.phase_status(id),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ,
    deleted_at      TIMESTAMPTZ,
    is_deleted      BOOLEAN NOT NULL DEFAULT false,
    sequence        INT NOT NULL DEFAULT 0,
    is_final        BOOLEAN NOT NULL DEFAULT false
);

CREATE TABLE competition.matches (
    id              INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    phase_id        INT NOT NULL REFERENCES competition.phases(id),
    player1_id      INT NOT NULL REFERENCES competition.players(id),
    player2_id      INT NOT NULL REFERENCES competition.players(id),
    score_player1   INT NOT NULL DEFAULT 0,
    score_player2   INT NOT NULL DEFAULT 0,
    winner_id       INT,
    match_state_id  SMALLINT NOT NULL REFERENCES competition.match_status(id),
    state_id        SMALLINT NOT NULL REFERENCES security.general_status(id),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at      TIMESTAMPTZ,
    deleted_at      TIMESTAMPTZ,
    is_deleted      BOOLEAN NOT NULL DEFAULT false
);

-- ─────────────────────────────────────────────────────────────────────────
-- Auditoría
-- ─────────────────────────────────────────────────────────────────────────

CREATE TABLE audit.system_logs (
    id          INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id     INT NULL REFERENCES security.users(id),
    action_type VARCHAR(50) NOT NULL,
    message     TEXT,
    ip_address  VARCHAR(50),
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now()
);
