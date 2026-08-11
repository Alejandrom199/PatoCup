-- ══════════════════════════════════════════════════════════════════════════
-- PatoCup — Funciones del schema security (equivalentes a los antiguos SPs)
--
-- Patrón de error-handling: cada función de "acción" (create/update/delete)
-- devuelve una fila con error_code/error_message (y new_id cuando aplica)
-- en vez de usar parámetros OUTPUT como en SQL Server. Esto permite llamarla
-- desde Dapper con QueryFirstOrDefaultAsync igual que una función de consulta.
-- ══════════════════════════════════════════════════════════════════════════

-- ── Catálogo de estados generales ───────────────────────────────────────────

CREATE OR REPLACE FUNCTION security.fn_general_status_get_all()
RETURNS TABLE(id SMALLINT, code CHAR(4), name VARCHAR(50), description VARCHAR(250))
LANGUAGE sql
SET search_path = ''
AS $$
    SELECT id, code, name, description
    FROM security.general_status
    ORDER BY id;
$$;

-- ── Autenticación ────────────────────────────────────────────────────────────
-- Caso especial: combina "consulta" (trae los datos del usuario) con
-- "acción" (error_code/error_message de validación) en una sola fila.

CREATE OR REPLACE FUNCTION security.fn_auth_login(p_username VARCHAR(50))
RETURNS TABLE(
    id INT, username VARCHAR(50), password VARCHAR(255), email VARCHAR(100),
    photo_url VARCHAR(500), role_id INT, role_name VARCHAR(50),
    state_id SMALLINT, state_name VARCHAR(50),
    error_code INT, error_message VARCHAR(200)
)
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
BEGIN
    IF NOT EXISTS (SELECT 1 FROM security.users WHERE username = p_username AND is_deleted = false) THEN
        RETURN QUERY SELECT NULL::INT, NULL::VARCHAR(50), NULL::VARCHAR(255), NULL::VARCHAR(100),
            NULL::VARCHAR(500), NULL::INT, NULL::VARCHAR(50), NULL::SMALLINT, NULL::VARCHAR(50),
            1, 'El usuario no existe.'::VARCHAR(200);
        RETURN;
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM security.users u
        INNER JOIN security.general_status gs ON u.state_id = gs.id
        WHERE u.username = p_username AND gs.code = 'ACTV' AND u.is_deleted = false
    ) THEN
        RETURN QUERY SELECT NULL::INT, NULL::VARCHAR(50), NULL::VARCHAR(255), NULL::VARCHAR(100),
            NULL::VARCHAR(500), NULL::INT, NULL::VARCHAR(50), NULL::SMALLINT, NULL::VARCHAR(50),
            2, 'Tu cuenta se encuentra inactiva o suspendida.'::VARCHAR(200);
        RETURN;
    END IF;

    RETURN QUERY
    SELECT u.id, u.username, u.password, u.email, u.photo_url,
           u.role_id, r.name, u.state_id, gs.name,
           0, 'Autenticado.'::VARCHAR(200)
    FROM security.users u
    INNER JOIN security.roles r ON u.role_id = r.id
    INNER JOIN security.general_status gs ON u.state_id = gs.id
    WHERE u.username = p_username;
EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT NULL::INT, NULL::VARCHAR(50), NULL::VARCHAR(255), NULL::VARCHAR(100),
        NULL::VARCHAR(500), NULL::INT, NULL::VARCHAR(50), NULL::SMALLINT, NULL::VARCHAR(50),
        99, SQLERRM::VARCHAR(200);
END;
$$;

CREATE OR REPLACE FUNCTION security.fn_users_change_password(p_id INT, p_new_password VARCHAR(255))
RETURNS TABLE(error_code INT, error_message VARCHAR(200))
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
BEGIN
    IF NOT EXISTS (SELECT 1 FROM security.users WHERE id = p_id AND is_deleted = false) THEN
        RETURN QUERY SELECT 1, 'Usuario no encontrado.'::VARCHAR(200);
        RETURN;
    END IF;

    UPDATE security.users
    SET password = p_new_password, updated_at = now()
    WHERE id = p_id;

    RETURN QUERY SELECT 0, 'Contraseña actualizada.'::VARCHAR(200);
EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT 99, SQLERRM::VARCHAR(200);
END;
$$;

-- ── Usuarios ─────────────────────────────────────────────────────────────────

CREATE OR REPLACE FUNCTION security.fn_users_create(
    p_role_id INT, p_username VARCHAR(50), p_password VARCHAR(255),
    p_email VARCHAR(100), p_photo_url VARCHAR(500) DEFAULT NULL
)
RETURNS TABLE(new_id INT, error_code INT, error_message VARCHAR(200))
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
DECLARE
    v_new_id INT;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM security.roles WHERE id = p_role_id AND is_deleted = false) THEN
        RETURN QUERY SELECT 0, 1, 'El Rol no existe.'::VARCHAR(200);
        RETURN;
    END IF;

    IF EXISTS (SELECT 1 FROM security.users WHERE username = p_username AND is_deleted = false) THEN
        RETURN QUERY SELECT 0, 2, 'Usuario ya ocupado.'::VARCHAR(200);
        RETURN;
    END IF;

    INSERT INTO security.users (role_id, username, password, email, photo_url, state_id, created_at, is_deleted)
    VALUES (p_role_id, p_username, p_password, p_email, p_photo_url, 1, now(), false)
    RETURNING id INTO v_new_id;

    RETURN QUERY SELECT v_new_id, 0, 'Usuario creado.'::VARCHAR(200);
EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT 0, 99, SQLERRM::VARCHAR(200);
END;
$$;

CREATE OR REPLACE FUNCTION security.fn_users_update(
    p_id INT, p_role_id INT, p_username VARCHAR(50), p_email VARCHAR(100),
    p_photo_url VARCHAR(500), p_state_id INT
)
RETURNS TABLE(error_code INT, error_message VARCHAR(200))
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
BEGIN
    IF NOT EXISTS (SELECT 1 FROM security.users WHERE id = p_id AND is_deleted = false) THEN
        RETURN QUERY SELECT 1, 'Usuario no existe.'::VARCHAR(200);
        RETURN;
    END IF;

    IF EXISTS (SELECT 1 FROM security.users WHERE (username = p_username OR email = p_email) AND id <> p_id AND is_deleted = false) THEN
        RETURN QUERY SELECT 3, 'Usuario o Email duplicado.'::VARCHAR(200);
        RETURN;
    END IF;

    UPDATE security.users
    SET role_id = p_role_id, username = p_username, email = p_email,
        photo_url = p_photo_url, state_id = p_state_id, updated_at = now()
    WHERE id = p_id;

    RETURN QUERY SELECT 0, 'Usuario actualizado.'::VARCHAR(200);
EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT 99, SQLERRM::VARCHAR(200);
END;
$$;

CREATE OR REPLACE FUNCTION security.fn_users_soft_delete(p_id INT)
RETURNS TABLE(error_code INT, error_message VARCHAR(200))
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
BEGIN
    -- ID 2 = INAC en general_status (igual que el SP original)
    UPDATE security.users
    SET is_deleted = true, deleted_at = now(), state_id = 2
    WHERE id = p_id;

    RETURN QUERY SELECT 0, 'Usuario eliminado.'::VARCHAR(200);
EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT 99, SQLERRM::VARCHAR(200);
END;
$$;

-- NUEVA: no existía en los scripts recuperados (sp_Users_Reactivate se
-- referenciaba en UserRepository.cs pero faltaba en usuarios.sql). Se
-- reconstruye siguiendo el mismo patrón que fn_tournaments_reactivate /
-- fn_phases_reactivate.
CREATE OR REPLACE FUNCTION security.fn_users_reactivate(p_id INT)
RETURNS TABLE(error_code INT, error_message VARCHAR(200))
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
BEGIN
    IF NOT EXISTS (SELECT 1 FROM security.users WHERE id = p_id AND is_deleted = true) THEN
        RETURN QUERY SELECT 1, 'El usuario no existe o no se encuentra eliminado.'::VARCHAR(200);
        RETURN;
    END IF;

    UPDATE security.users
    SET is_deleted = false, deleted_at = NULL, updated_at = now(), state_id = 1
    WHERE id = p_id;

    RETURN QUERY SELECT 0, 'Usuario reactivado con éxito.'::VARCHAR(200);
EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT 99, SQLERRM::VARCHAR(200);
END;
$$;

CREATE OR REPLACE FUNCTION security.fn_users_get_all(
    p_page_number INT DEFAULT 1, p_page_size INT DEFAULT 10,
    p_filter_username VARCHAR(50) DEFAULT NULL, p_filter_role_id INT DEFAULT NULL
)
RETURNS TABLE(
    id INT, role_id INT, role_name VARCHAR(50), username VARCHAR(50), email VARCHAR(100),
    photo_url VARCHAR(500), state_id SMALLINT, state_name VARCHAR(50), created_at TIMESTAMPTZ,
    total_records BIGINT
)
LANGUAGE sql
SET search_path = ''
AS $$
    SELECT
        u.id, u.role_id, r.name, u.username, u.email, u.photo_url,
        u.state_id, gs.name, u.created_at,
        COUNT(*) OVER() AS total_records
    FROM security.users u
    INNER JOIN security.roles r ON u.role_id = r.id
    INNER JOIN security.general_status gs ON u.state_id = gs.id
    WHERE u.is_deleted = false
      AND (COALESCE(p_filter_username, '') = '' OR u.username ILIKE '%' || p_filter_username || '%')
      AND (p_filter_role_id IS NULL OR u.role_id = p_filter_role_id)
    ORDER BY u.id DESC
    LIMIT p_page_size OFFSET (p_page_number - 1) * p_page_size;
$$;

CREATE OR REPLACE FUNCTION security.fn_users_get_by_id(p_id INT)
RETURNS TABLE(id INT, role_id INT, username VARCHAR(50), email VARCHAR(100), photo_url VARCHAR(500), state_id SMALLINT)
LANGUAGE sql
SET search_path = ''
AS $$
    SELECT id, role_id, username, email, photo_url, state_id
    FROM security.users
    WHERE id = p_id AND is_deleted = false;
$$;

-- ── Roles y permisos (schema completo en BD; sin repositorio/controlador
-- C# todavía — se deja migrado para no perder esa parte de la base) ─────────

CREATE OR REPLACE FUNCTION security.fn_roles_create(p_name VARCHAR(50), p_description VARCHAR(200))
RETURNS TABLE(new_id INT, error_code INT, error_message VARCHAR(200))
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
DECLARE
    v_new_id INT;
BEGIN
    IF EXISTS (SELECT 1 FROM security.roles WHERE name = p_name AND is_deleted = false) THEN
        RETURN QUERY SELECT 0, 1, 'Ya existe un rol con ese nombre.'::VARCHAR(200);
        RETURN;
    END IF;

    INSERT INTO security.roles (name, description, state_id, created_at, is_deleted)
    VALUES (p_name, p_description, 1, now(), false)
    RETURNING id INTO v_new_id;

    RETURN QUERY SELECT v_new_id, 0, 'Rol creado exitosamente.'::VARCHAR(200);
EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT 0, 99, SQLERRM::VARCHAR(200);
END;
$$;

CREATE OR REPLACE FUNCTION security.fn_roles_update(
    p_id INT, p_name VARCHAR(50), p_description VARCHAR(200), p_state_id INT
)
RETURNS TABLE(error_code INT, error_message VARCHAR(200))
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
BEGIN
    IF NOT EXISTS (SELECT 1 FROM security.roles WHERE id = p_id AND is_deleted = false) THEN
        RETURN QUERY SELECT 1, 'El rol no existe.'::VARCHAR(200);
        RETURN;
    END IF;

    IF EXISTS (SELECT 1 FROM security.roles WHERE name = p_name AND id <> p_id AND is_deleted = false) THEN
        RETURN QUERY SELECT 2, 'Nombre de rol duplicado.'::VARCHAR(200);
        RETURN;
    END IF;

    UPDATE security.roles
    SET name = p_name, description = p_description, state_id = p_state_id, updated_at = now()
    WHERE id = p_id;

    RETURN QUERY SELECT 0, 'Rol actualizado.'::VARCHAR(200);
EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT 99, SQLERRM::VARCHAR(200);
END;
$$;

CREATE OR REPLACE FUNCTION security.fn_roles_get_all(
    p_page_number INT DEFAULT 1, p_page_size INT DEFAULT 10, p_filter_name VARCHAR(50) DEFAULT NULL
)
RETURNS TABLE(
    id INT, name VARCHAR(50), description VARCHAR(200), state_id SMALLINT, state_name VARCHAR(50),
    created_at TIMESTAMPTZ, updated_at TIMESTAMPTZ, total_records BIGINT
)
LANGUAGE sql
SET search_path = ''
AS $$
    SELECT r.id, r.name, r.description, r.state_id, gs.name, r.created_at, r.updated_at,
           COUNT(*) OVER() AS total_records
    FROM security.roles r
    INNER JOIN security.general_status gs ON r.state_id = gs.id
    WHERE r.is_deleted = false
      AND (COALESCE(p_filter_name, '') = '' OR r.name ILIKE '%' || p_filter_name || '%')
    ORDER BY r.id DESC
    LIMIT p_page_size OFFSET (p_page_number - 1) * p_page_size;
$$;

CREATE OR REPLACE FUNCTION security.fn_roles_get_select()
RETURNS TABLE(id INT, name VARCHAR(50))
LANGUAGE sql
SET search_path = ''
AS $$
    SELECT r.id, r.name
    FROM security.roles r
    INNER JOIN security.general_status gs ON r.state_id = gs.id
    WHERE r.is_deleted = false AND gs.code = 'ACTV'
    ORDER BY r.name ASC;
$$;

CREATE OR REPLACE FUNCTION security.fn_roles_get_by_id(p_id INT)
RETURNS TABLE(id INT, name VARCHAR(50), description VARCHAR(200), state_id SMALLINT)
LANGUAGE sql
SET search_path = ''
AS $$
    SELECT id, name, description, state_id
    FROM security.roles
    WHERE id = p_id AND is_deleted = false;
$$;

CREATE OR REPLACE FUNCTION security.fn_security_get_permissions_by_role(p_role_id INT)
RETURNS TABLE(code VARCHAR(50))
LANGUAGE sql
SET search_path = ''
AS $$
    SELECT DISTINCT a.code
    FROM security.role_permissions rp
    INNER JOIN security.option_actions oa ON rp.option_action_id = oa.id
    INNER JOIN security.actions a ON oa.action_id = a.id
    WHERE rp.role_id = p_role_id;
$$;

CREATE OR REPLACE FUNCTION security.fn_security_assign_permission(
    p_role_id INT, p_option_action_id INT, p_is_assigned BOOLEAN
)
RETURNS TABLE(error_code INT, error_message VARCHAR(200))
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
BEGIN
    IF p_is_assigned THEN
        IF NOT EXISTS (SELECT 1 FROM security.role_permissions WHERE role_id = p_role_id AND option_action_id = p_option_action_id) THEN
            INSERT INTO security.role_permissions (role_id, option_action_id) VALUES (p_role_id, p_option_action_id);
        END IF;
        RETURN QUERY SELECT 0, 'Permiso asignado.'::VARCHAR(200);
    ELSE
        DELETE FROM security.role_permissions WHERE role_id = p_role_id AND option_action_id = p_option_action_id;
        RETURN QUERY SELECT 0, 'Permiso revocado.'::VARCHAR(200);
    END IF;
EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT 99, SQLERRM::VARCHAR(200);
END;
$$;

-- ── Menú dinámico por usuario ────────────────────────────────────────────────

CREATE OR REPLACE FUNCTION security.fn_security_get_menu_by_user_id(p_user_id INT)
RETURNS TABLE(
    menu_id INT, menu_name VARCHAR(50), menu_icon VARCHAR(50), menu_order INT,
    option_id INT, option_name VARCHAR(50), option_route VARCHAR(100), option_icon VARCHAR(50)
)
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
DECLARE
    v_role_id INT;
BEGIN
    SELECT role_id INTO v_role_id FROM security.users WHERE id = p_user_id;

    RETURN QUERY
    SELECT DISTINCT
        m.id, m.name, m.icon, m."order",
        o.id, o.name, o.route, o.icon
    FROM security.menus m
    INNER JOIN security.options o ON m.id = o.menu_id
    INNER JOIN security.option_actions oa ON o.id = oa.option_id
    INNER JOIN security.actions a ON oa.action_id = a.id
    INNER JOIN security.role_permissions rp ON oa.id = rp.option_action_id
    WHERE rp.role_id = v_role_id
      AND a.code = 'LIST'
      AND m.is_deleted = false
      AND o.is_deleted = false
    ORDER BY m."order", o.id;
END;
$$;
