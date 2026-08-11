-- ══════════════════════════════════════════════════════════════════════════
-- PatoCup — Funciones del schema competition (equivalentes a los antiguos SPs)
-- Mismo patrón que 002_functions_security.sql: las funciones de "acción"
-- devuelven una fila con error_code/error_message (y new_id si aplica).
-- ══════════════════════════════════════════════════════════════════════════

-- ── Catálogos de estado ──────────────────────────────────────────────────────

CREATE OR REPLACE FUNCTION competition.fn_player_status_get_all()
RETURNS TABLE(id SMALLINT, code CHAR(4), name VARCHAR(50), description VARCHAR(250))
LANGUAGE sql
SET search_path = ''
AS $$
    SELECT id, code, name, description
    FROM competition.player_status
    WHERE code <> 'BANN'
    ORDER BY id;
$$;

CREATE OR REPLACE FUNCTION competition.fn_tournament_status_get_all()
RETURNS TABLE(id SMALLINT, code CHAR(4), name VARCHAR(50), description VARCHAR(250))
LANGUAGE sql
SET search_path = ''
AS $$
    SELECT id, code, name, description
    FROM competition.tournament_status
    ORDER BY id;
$$;

CREATE OR REPLACE FUNCTION competition.fn_phase_status_get_all()
RETURNS TABLE(id SMALLINT, code CHAR(4), name VARCHAR(50))
LANGUAGE sql
SET search_path = ''
AS $$
    SELECT id, code, name
    FROM competition.phase_status
    ORDER BY id;
$$;

CREATE OR REPLACE FUNCTION competition.fn_match_status_get_all()
RETURNS TABLE(id SMALLINT, code CHAR(4), name VARCHAR(50))
LANGUAGE sql
SET search_path = ''
AS $$
    SELECT id, code, name
    FROM competition.match_status
    ORDER BY id;
$$;

-- ── Torneos ──────────────────────────────────────────────────────────────────

CREATE OR REPLACE FUNCTION competition.fn_tournaments_create(
    p_name VARCHAR(100), p_description VARCHAR(500),
    p_start_date TIMESTAMPTZ, p_end_date TIMESTAMPTZ
)
RETURNS TABLE(new_id INT, error_code INT, error_message VARCHAR(200))
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
DECLARE
    v_new_id INT;
BEGIN
    IF p_end_date < p_start_date THEN
        RETURN QUERY SELECT 0, 1, 'La fecha fin no puede ser menor a la de inicio.'::VARCHAR(200);
        RETURN;
    END IF;

    INSERT INTO competition.tournaments
        (name, description, start_date, end_date, tournament_state_id, state_id, created_at, is_deleted)
    VALUES
        (p_name, p_description, p_start_date, p_end_date, 1, 1, now(), false)
    RETURNING id INTO v_new_id;

    RETURN QUERY SELECT v_new_id, 0, 'Torneo creado.'::VARCHAR(200);
EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT 0, 99, SQLERRM::VARCHAR(200);
END;
$$;

CREATE OR REPLACE FUNCTION competition.fn_tournaments_update(
    p_id INT, p_name VARCHAR(100), p_description VARCHAR(500),
    p_start_date TIMESTAMPTZ, p_end_date TIMESTAMPTZ,
    p_tournament_state_id INT, p_state_id INT
)
RETURNS TABLE(error_code INT, error_message VARCHAR(200))
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
BEGIN
    IF NOT EXISTS (SELECT 1 FROM competition.tournaments WHERE id = p_id AND is_deleted = false) THEN
        RETURN QUERY SELECT 1, 'Torneo no existe.'::VARCHAR(200);
        RETURN;
    END IF;

    IF p_end_date < p_start_date THEN
        RETURN QUERY SELECT 2, 'La fecha fin no puede ser menor a la de inicio.'::VARCHAR(200);
        RETURN;
    END IF;

    UPDATE competition.tournaments
    SET name = p_name, description = p_description, start_date = p_start_date, end_date = p_end_date,
        tournament_state_id = p_tournament_state_id, state_id = p_state_id, updated_at = now()
    WHERE id = p_id;

    RETURN QUERY SELECT 0, 'Torneo actualizado correctamente.'::VARCHAR(200);
EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT 99, SQLERRM::VARCHAR(200);
END;
$$;

CREATE OR REPLACE FUNCTION competition.fn_tournaments_get_all(
    p_page_number INT DEFAULT 1, p_page_size INT DEFAULT 10,
    p_name VARCHAR(100) DEFAULT NULL, p_description VARCHAR(500) DEFAULT NULL,
    p_start_date TIMESTAMPTZ DEFAULT NULL, p_end_date TIMESTAMPTZ DEFAULT NULL,
    p_tournament_state_id INT DEFAULT NULL
)
RETURNS TABLE(
    id INT, name VARCHAR(100), description VARCHAR(500), start_date TIMESTAMPTZ, end_date TIMESTAMPTZ,
    tournament_state_id SMALLINT, tournament_state_name VARCHAR(50),
    state_id SMALLINT, state_name VARCHAR(50), is_public BOOLEAN, total_records BIGINT
)
LANGUAGE sql
SET search_path = ''
AS $$
    SELECT
        t.id, t.name, t.description, t.start_date, t.end_date,
        t.tournament_state_id, c.name, t.state_id, g.name, t.is_public,
        COUNT(*) OVER() AS total_records
    FROM competition.tournaments t
    INNER JOIN competition.tournament_status c ON t.tournament_state_id = c.id
    INNER JOIN security.general_status g ON t.state_id = g.id
    WHERE
        t.is_deleted = false
        AND (p_name IS NULL OR t.name ILIKE '%' || p_name || '%')
        AND (p_description IS NULL OR t.description ILIKE '%' || p_description || '%')
        AND (p_start_date IS NULL OR t.start_date >= p_start_date)
        AND (p_end_date IS NULL OR t.end_date <= p_end_date)
        AND (p_tournament_state_id IS NULL OR t.tournament_state_id = p_tournament_state_id)
    ORDER BY t.id DESC
    LIMIT p_page_size OFFSET (p_page_number - 1) * p_page_size;
$$;

CREATE OR REPLACE FUNCTION competition.fn_tournaments_get_by_id(p_id INT)
RETURNS TABLE(
    id INT, name VARCHAR(100), description VARCHAR(500), start_date TIMESTAMPTZ, end_date TIMESTAMPTZ,
    tournament_state_id SMALLINT, tournament_state_name VARCHAR(50),
    state_id SMALLINT, state_name VARCHAR(50), is_public BOOLEAN
)
LANGUAGE sql
SET search_path = ''
AS $$
    SELECT
        t.id, t.name, t.description, t.start_date, t.end_date,
        t.tournament_state_id, c.name, t.state_id, g.name, t.is_public
    FROM competition.tournaments t
    INNER JOIN competition.tournament_status c ON t.tournament_state_id = c.id
    INNER JOIN security.general_status g ON t.state_id = g.id
    WHERE t.id = p_id AND t.is_deleted = false;
$$;

CREATE OR REPLACE FUNCTION competition.fn_tournaments_soft_delete(p_id INT)
RETURNS TABLE(error_code INT, error_message VARCHAR(200))
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
BEGIN
    IF NOT EXISTS (SELECT 1 FROM competition.tournaments WHERE id = p_id AND is_deleted = false) THEN
        RETURN QUERY SELECT 1, 'El torneo no existe o ya ha sido eliminado.'::VARCHAR(200);
        RETURN;
    END IF;

    UPDATE competition.tournaments
    SET is_deleted = true, deleted_at = now(), state_id = 2
    WHERE id = p_id;

    RETURN QUERY SELECT 0, 'Torneo eliminado lógicamente.'::VARCHAR(200);
EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT 99, SQLERRM::VARCHAR(200);
END;
$$;

CREATE OR REPLACE FUNCTION competition.fn_tournaments_reactivate(p_id INT)
RETURNS TABLE(error_code INT, error_message VARCHAR(200))
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
BEGIN
    IF NOT EXISTS (SELECT 1 FROM competition.tournaments WHERE id = p_id AND is_deleted = true) THEN
        RETURN QUERY SELECT 1, 'El torneo no existe o no se encuentra en estado eliminado.'::VARCHAR(200);
        RETURN;
    END IF;

    UPDATE competition.tournaments
    SET is_deleted = false, deleted_at = NULL, updated_at = now(), state_id = 1
    WHERE id = p_id;

    RETURN QUERY SELECT 0, 'Torneo reactivado con éxito.'::VARCHAR(200);
EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT 99, SQLERRM::VARCHAR(200);
END;
$$;

CREATE OR REPLACE FUNCTION competition.fn_tournaments_set_public(p_id INT)
RETURNS TABLE(error_code INT, error_message VARCHAR(200))
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
BEGIN
    UPDATE competition.tournaments SET is_public = false;

    UPDATE competition.tournaments
    SET is_public = true, updated_at = now()
    WHERE id = p_id AND is_deleted = false;

    RETURN QUERY SELECT 0, 'Torneo publicado exitosamente.'::VARCHAR(200);
EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT 99, SQLERRM::VARCHAR(200);
END;
$$;

CREATE OR REPLACE FUNCTION competition.fn_tournaments_get_public_active()
RETURNS TABLE(id INT, name VARCHAR(100), description VARCHAR(500), is_public BOOLEAN, start_date TIMESTAMPTZ, end_date TIMESTAMPTZ)
LANGUAGE sql
SET search_path = ''
AS $$
    SELECT t.id, t.name, t.description, t.is_public, t.start_date, t.end_date
    FROM competition.tournaments t
    WHERE t.is_public = true AND t.is_deleted = false;
$$;

-- Nota de columnas: el último campo (match_phase_name) es un duplicado
-- deliberado de phase_name -- así estaba también en el SP original de SQL
-- Server ("para que el Mapper sepa si es final"), y el Match del C# lo
-- espera como parte de su segmento en el multi-mapping de Dapper.
CREATE OR REPLACE FUNCTION competition.fn_tournaments_get_public_bracket()
RETURNS TABLE(
    tournament_id INT, tournament_name VARCHAR(100),
    phase_id INT, phase_name VARCHAR(50), sequence INT, is_final BOOLEAN,
    match_id INT, player1_name VARCHAR(50), player2_name VARCHAR(50),
    score_player1 INT, score_player2 INT, match_phase_name VARCHAR(50)
)
LANGUAGE sql
SET search_path = ''
AS $$
    SELECT
        t.id, t.name,
        p.id, p.name, COALESCE(p.sequence, 0), p.is_final,
        m.id,
        COALESCE(p1.nickname, 'TBD'), COALESCE(p2.nickname, 'TBD'),
        m.score_player1, m.score_player2,
        p.name
    FROM competition.tournaments t
    LEFT JOIN competition.phases p ON t.id = p.tournament_id AND p.is_deleted = false
    LEFT JOIN competition.matches m ON p.id = m.phase_id AND m.is_deleted = false
    LEFT JOIN competition.players p1 ON m.player1_id = p1.id
    LEFT JOIN competition.players p2 ON m.player2_id = p2.id
    WHERE t.is_public = true AND t.is_deleted = false
    ORDER BY p.sequence ASC;
$$;

-- ── Fases ────────────────────────────────────────────────────────────────────

CREATE OR REPLACE FUNCTION competition.fn_phases_get_all()
RETURNS TABLE(
    id INT, name VARCHAR(50), sequence INT, is_final BOOLEAN,
    tournament_id INT, tournament_name VARCHAR(100),
    phase_state_id SMALLINT, phase_state_name VARCHAR(50),
    state_id SMALLINT, state_name VARCHAR(50), total_records BIGINT
)
LANGUAGE sql
SET search_path = ''
AS $$
    SELECT
        p.id, p.name, p.sequence, p.is_final,
        p.tournament_id, t.name,
        p.phase_state_id, ps.name,
        p.state_id, gs.name,
        COUNT(*) OVER() AS total_records
    FROM competition.phases p
    INNER JOIN competition.tournaments t ON p.tournament_id = t.id
    INNER JOIN competition.phase_status ps ON p.phase_state_id = ps.id
    INNER JOIN security.general_status gs ON p.state_id = gs.id
    WHERE p.is_deleted = false
    ORDER BY p.sequence ASC, p.id DESC;
$$;

CREATE OR REPLACE FUNCTION competition.fn_phases_get_by_id(p_id INT)
RETURNS TABLE(
    id INT, name VARCHAR(50), sequence INT, is_final BOOLEAN,
    tournament_id INT, tournament_name VARCHAR(100),
    phase_state_id SMALLINT, phase_state_name VARCHAR(50),
    state_id SMALLINT, state_name VARCHAR(50)
)
LANGUAGE sql
SET search_path = ''
AS $$
    SELECT
        p.id, p.name, p.sequence, p.is_final,
        p.tournament_id, t.name,
        p.phase_state_id, ps.name,
        p.state_id, gs.name
    FROM competition.phases p
    INNER JOIN competition.tournaments t ON p.tournament_id = t.id
    INNER JOIN competition.phase_status ps ON p.phase_state_id = ps.id
    INNER JOIN security.general_status gs ON p.state_id = gs.id
    WHERE p.is_deleted = false AND p.id = p_id;
$$;

CREATE OR REPLACE FUNCTION competition.fn_phases_create(p_tournament_id INT, p_name VARCHAR(50))
RETURNS TABLE(new_id INT, error_code INT, error_message VARCHAR(200))
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
DECLARE
    v_new_id INT;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM competition.tournaments WHERE id = p_tournament_id AND is_deleted = false) THEN
        RETURN QUERY SELECT 0, 5, 'El torneo especificado no existe o está eliminado.'::VARCHAR(200);
        RETURN;
    END IF;

    INSERT INTO competition.phases (tournament_id, name, phase_state_id, state_id, created_at, is_deleted, sequence)
    VALUES (p_tournament_id, p_name, 1, 1, now(), false, -1)
    RETURNING id INTO v_new_id;

    RETURN QUERY SELECT v_new_id, 0, 'Fase creada exitosamente.'::VARCHAR(200);
EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT 0, 99, SQLERRM::VARCHAR(200);
END;
$$;

-- Nota: SQL Server usaba BEGIN TRANSACTION/COMMIT/ROLLBACK explícito. En
-- Postgres una función PL/pgSQL ya es atómica dentro de la transacción que
-- la invoca -- si el bloque EXCEPTION no la atrapa, todo su efecto se
-- revierte solo. No hace falta manejo de transacción explícito.
CREATE OR REPLACE FUNCTION competition.fn_phases_update(
    p_id INT, p_name VARCHAR(50), p_phase_state_id INT, p_state_id INT, p_sequence INT
)
RETURNS TABLE(error_code INT, error_message VARCHAR(200))
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
DECLARE
    v_tournament_id INT;
BEGIN
    SELECT tournament_id INTO v_tournament_id FROM competition.phases WHERE id = p_id;

    UPDATE competition.phases
    SET name = p_name, phase_state_id = p_phase_state_id, state_id = p_state_id,
        sequence = p_sequence, updated_at = now()
    WHERE id = p_id;

    IF p_sequence > 0 THEN
        WITH ordered_phases AS (
            SELECT id, sequence,
                ROW_NUMBER() OVER (
                    ORDER BY
                        CASE WHEN id = p_id THEN -1 ELSE 0 END,
                        sequence ASC,
                        updated_at DESC
                ) AS new_rank
            FROM competition.phases
            WHERE tournament_id = v_tournament_id AND sequence > 0 AND is_deleted = false
        )
        UPDATE competition.phases ph
        SET sequence = op.new_rank
        FROM ordered_phases op
        WHERE ph.id = op.id;
    END IF;

    RETURN QUERY SELECT 0, 'Fase actualizada correctamente.'::VARCHAR(200);
EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT 99, SQLERRM::VARCHAR(200);
END;
$$;

CREATE OR REPLACE FUNCTION competition.fn_phases_soft_delete(p_id INT)
RETURNS TABLE(error_code INT, error_message VARCHAR(200))
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
BEGIN
    IF NOT EXISTS (SELECT 1 FROM competition.phases WHERE id = p_id AND is_deleted = false) THEN
        RETURN QUERY SELECT 1, 'La fase no existe o ya ha sido eliminada.'::VARCHAR(200);
        RETURN;
    END IF;

    UPDATE competition.phases
    SET is_deleted = true, deleted_at = now(), state_id = 2
    WHERE id = p_id;

    RETURN QUERY SELECT 0, 'Fase eliminada lógicamente.'::VARCHAR(200);
EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT 99, SQLERRM::VARCHAR(200);
END;
$$;

CREATE OR REPLACE FUNCTION competition.fn_phases_reactivate(p_id INT)
RETURNS TABLE(error_code INT, error_message VARCHAR(200))
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
BEGIN
    IF NOT EXISTS (SELECT 1 FROM competition.phases WHERE id = p_id AND is_deleted = true) THEN
        RETURN QUERY SELECT 4, 'La fase no existe o no está eliminada.'::VARCHAR(200);
        RETURN;
    END IF;

    UPDATE competition.phases
    SET is_deleted = false, deleted_at = NULL, updated_at = now(), state_id = 1
    WHERE id = p_id;

    RETURN QUERY SELECT 0, 'Fase reactivada con éxito.'::VARCHAR(200);
EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT 99, SQLERRM::VARCHAR(200);
END;
$$;

CREATE OR REPLACE FUNCTION competition.fn_phases_get_by_tournament_id(p_tournament_id INT)
RETURNS TABLE(
    id INT, name VARCHAR(50), sequence INT, is_final BOOLEAN,
    tournament_id INT, tournament_name VARCHAR(100),
    phase_state_id SMALLINT, phase_state_name VARCHAR(50),
    state_id SMALLINT, state_name VARCHAR(50), total_records BIGINT
)
LANGUAGE sql
SET search_path = ''
AS $$
    SELECT
        p.id, p.name, p.sequence, p.is_final,
        p.tournament_id, t.name,
        p.phase_state_id, ps.name,
        p.state_id, gs.name,
        COUNT(*) OVER() AS total_records
    FROM competition.phases p
    INNER JOIN competition.tournaments t ON p.tournament_id = t.id
    INNER JOIN competition.phase_status ps ON p.phase_state_id = ps.id
    INNER JOIN security.general_status gs ON p.state_id = gs.id
    WHERE p.tournament_id = p_tournament_id AND p.is_deleted = false
    ORDER BY p.sequence DESC;
$$;

CREATE OR REPLACE FUNCTION competition.fn_phases_set_final(p_tournament_id INT, p_phase_id INT)
RETURNS TABLE(error_code INT, error_message VARCHAR(200))
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
DECLARE
    v_match_count INT;
BEGIN
    SELECT COUNT(*) INTO v_match_count
    FROM competition.matches
    WHERE phase_id = p_phase_id AND is_deleted = false;

    IF v_match_count <> 1 THEN
        RETURN QUERY SELECT 6, 'Solo puedes marcar como Final una fase que tenga exactamente 1 partido.'::VARCHAR(200);
        RETURN;
    END IF;

    UPDATE competition.phases SET is_final = false WHERE tournament_id = p_tournament_id;
    UPDATE competition.phases SET is_final = true WHERE id = p_phase_id;

    RETURN QUERY SELECT 0, '¡Fase Final establecida con éxito!'::VARCHAR(200);
EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT 99, SQLERRM::VARCHAR(200);
END;
$$;

-- ── Partidas ─────────────────────────────────────────────────────────────────

CREATE OR REPLACE FUNCTION competition.fn_matches_create(p_phase_id INT, p_player1_id INT, p_player2_id INT)
RETURNS TABLE(new_id INT, error_code INT, error_message VARCHAR(200))
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
DECLARE
    v_new_id INT;
BEGIN
    IF p_player1_id = p_player2_id THEN
        RETURN QUERY SELECT 0, 1, 'Los jugadores deben ser diferentes.'::VARCHAR(200);
        RETURN;
    END IF;

    INSERT INTO competition.matches
        (phase_id, player1_id, player2_id, score_player1, score_player2, winner_id, match_state_id, state_id, created_at, is_deleted)
    VALUES
        (p_phase_id, p_player1_id, p_player2_id, 0, 0, NULL, 1, 1, now(), false)
    RETURNING id INTO v_new_id;

    RETURN QUERY SELECT v_new_id, 0, 'Partida creada.'::VARCHAR(200);
EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT 0, 99, SQLERRM::VARCHAR(200);
END;
$$;

CREATE OR REPLACE FUNCTION competition.fn_matches_update(
    p_id INT, p_player1_id INT, p_player2_id INT, p_match_state_id INT, p_state_id INT
)
RETURNS TABLE(error_code INT, error_message VARCHAR(200))
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
BEGIN
    IF NOT EXISTS (SELECT 1 FROM competition.matches WHERE id = p_id AND is_deleted = false) THEN
        RETURN QUERY SELECT 1, 'Partida no existe.'::VARCHAR(200);
        RETURN;
    END IF;

    IF p_player1_id = p_player2_id THEN
        RETURN QUERY SELECT 2, 'Los jugadores deben ser diferentes.'::VARCHAR(200);
        RETURN;
    END IF;

    UPDATE competition.matches
    SET player1_id = p_player1_id, player2_id = p_player2_id,
        match_state_id = p_match_state_id, state_id = p_state_id, updated_at = now()
    WHERE id = p_id;

    RETURN QUERY SELECT 0, 'Partida reprogramada correctamente.'::VARCHAR(200);
EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT 99, SQLERRM::VARCHAR(200);
END;
$$;

CREATE OR REPLACE FUNCTION competition.fn_matches_register_result(
    p_id INT, p_score_player1 INT, p_score_player2 INT, p_winner_id INT DEFAULT NULL
)
RETURNS TABLE(error_code INT, error_message VARCHAR(200))
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
DECLARE
    v_real_p1 INT;
    v_real_p2 INT;
BEGIN
    SELECT player1_id, player2_id INTO v_real_p1, v_real_p2
    FROM competition.matches WHERE id = p_id AND is_deleted = false;

    IF v_real_p1 IS NULL THEN
        RETURN QUERY SELECT 1, 'Partida no existe.'::VARCHAR(200);
        RETURN;
    END IF;

    IF p_winner_id IS NOT NULL AND (p_winner_id <> v_real_p1 AND p_winner_id <> v_real_p2) THEN
        RETURN QUERY SELECT 1, 'El ganador debe ser uno de los participantes.'::VARCHAR(200);
        RETURN;
    END IF;

    UPDATE competition.matches
    SET score_player1 = p_score_player1, score_player2 = p_score_player2,
        winner_id = p_winner_id, updated_at = now()
    WHERE id = p_id;

    RETURN QUERY SELECT 0, 'Resultado registrado.'::VARCHAR(200);
EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT 99, SQLERRM::VARCHAR(200);
END;
$$;

CREATE OR REPLACE FUNCTION competition.fn_matches_get_by_phase_id(p_phase_id INT)
RETURNS TABLE(
    id INT, phase_id INT, phase_name VARCHAR(50),
    player1_id INT, player1_name VARCHAR(50),
    player2_id INT, player2_name VARCHAR(50),
    score_player1 INT, score_player2 INT,
    winner_id INT, winner_name VARCHAR(50),
    match_state_id SMALLINT, match_state_name VARCHAR(50),
    state_id SMALLINT, state_name VARCHAR(50)
)
LANGUAGE sql
SET search_path = ''
AS $$
    SELECT
        m.id,
        m.phase_id, p.name,
        m.player1_id, p1.nickname,
        m.player2_id, p2.nickname,
        m.score_player1, m.score_player2,
        m.winner_id, pw.nickname,
        m.match_state_id, c.name,
        m.state_id, gs.name
    FROM competition.matches m
    INNER JOIN competition.phases p ON m.phase_id = p.id
    INNER JOIN competition.match_status c ON m.match_state_id = c.id
    INNER JOIN competition.players p1 ON m.player1_id = p1.id
    INNER JOIN competition.players p2 ON m.player2_id = p2.id
    LEFT JOIN competition.players pw ON m.winner_id = pw.id
    INNER JOIN security.general_status gs ON m.state_id = gs.id
    WHERE m.phase_id = p_phase_id AND m.is_deleted = false
    ORDER BY m.id ASC;
$$;

CREATE OR REPLACE FUNCTION competition.fn_matches_soft_delete(p_id INT)
RETURNS TABLE(error_code INT, error_message VARCHAR(200))
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
BEGIN
    UPDATE competition.matches SET is_deleted = true, deleted_at = now(), state_id = 2 WHERE id = p_id;
    RETURN QUERY SELECT 0, 'Partida eliminada.'::VARCHAR(200);
EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT 99, SQLERRM::VARCHAR(200);
END;
$$;

-- ── Jugadores ────────────────────────────────────────────────────────────────

CREATE OR REPLACE FUNCTION competition.fn_players_public_submit(
    p_nickname VARCHAR(50), p_game_id VARCHAR(20), p_registration_ip VARCHAR(45)
)
RETURNS TABLE(error_code INT, error_message VARCHAR(200))
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
BEGIN
    IF EXISTS (SELECT 1 FROM competition.players WHERE registration_ip = p_registration_ip AND is_deleted = false) THEN
        RETURN QUERY SELECT 1, 'Ya existe una solicitud enviada desde esta conexión.'::VARCHAR(200);
        RETURN;
    END IF;

    IF EXISTS (SELECT 1 FROM competition.players WHERE game_id = p_game_id AND is_deleted = false) THEN
        RETURN QUERY SELECT 2, 'Este ID de juego ya se encuentra registrado.'::VARCHAR(200);
        RETURN;
    END IF;

    INSERT INTO competition.players (nickname, game_id, registration_ip, state_id, player_state_id, created_at, is_deleted)
    VALUES (p_nickname, p_game_id, p_registration_ip, 1, 1, now(), false);

    RETURN QUERY SELECT 0, 'Solicitud enviada. Espera la aprobación del administrador.'::VARCHAR(200);
EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT 99, SQLERRM::VARCHAR(200);
END;
$$;

CREATE OR REPLACE FUNCTION competition.fn_players_admin_list(
    p_filter_text VARCHAR(50) DEFAULT NULL, p_page_number INT DEFAULT 1, p_page_size INT DEFAULT 10
)
RETURNS TABLE(
    id INT, nickname VARCHAR(50), game_id VARCHAR(20), registration_ip VARCHAR(45), created_at TIMESTAMPTZ,
    player_state_id SMALLINT, player_state_name VARCHAR(50),
    state_id SMALLINT, state_name VARCHAR(50), total_records BIGINT
)
LANGUAGE sql
SET search_path = ''
AS $$
    SELECT
        p.id, p.nickname, p.game_id, p.registration_ip, p.created_at,
        p.player_state_id, stat.name,
        p.state_id, gen_state.name,
        COUNT(*) OVER() AS total_records
    FROM competition.players p
    INNER JOIN competition.player_status stat ON p.player_state_id = stat.id
    INNER JOIN security.general_status gen_state ON p.state_id = gen_state.id
    WHERE p.is_deleted = false
      AND (COALESCE(p_filter_text, '') = '' OR p.nickname ILIKE '%' || p_filter_text || '%' OR p.game_id ILIKE '%' || p_filter_text || '%')
    ORDER BY p.created_at DESC
    LIMIT p_page_size OFFSET (p_page_number - 1) * p_page_size;
$$;

CREATE OR REPLACE FUNCTION competition.fn_players_process_request(p_player_id INT, p_new_player_state_id INT)
RETURNS TABLE(error_code INT, error_message VARCHAR(200))
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
DECLARE
    v_rowcount INT;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM competition.player_status WHERE id = p_new_player_state_id) THEN
        RETURN QUERY SELECT 1, 'El ID de estado proporcionado no existe.'::VARCHAR(200);
        RETURN;
    END IF;

    IF p_new_player_state_id IN (2, 3) THEN
        UPDATE competition.players
        SET
            player_state_id = p_new_player_state_id,
            updated_at = now(),
            is_deleted = CASE WHEN p_new_player_state_id = 3 THEN true ELSE is_deleted END,
            deleted_at = CASE WHEN p_new_player_state_id = 3 THEN now() ELSE deleted_at END,
            state_id = CASE WHEN p_new_player_state_id = 3 THEN 2 ELSE state_id END
        WHERE id = p_player_id;

        GET DIAGNOSTICS v_rowcount = ROW_COUNT;
        IF v_rowcount = 0 THEN
            RETURN QUERY SELECT 2, 'El jugador no existe.'::VARCHAR(200);
            RETURN;
        END IF;
    END IF;

    RETURN QUERY SELECT 0, 'Solicitud procesada correctamente.'::VARCHAR(200);
EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT 99, SQLERRM::VARCHAR(200);
END;
$$;

CREATE OR REPLACE FUNCTION competition.fn_players_update(
    p_id INT, p_nickname VARCHAR(50), p_game_id VARCHAR(20), p_state_id INT
)
RETURNS TABLE(error_code INT, error_message VARCHAR(200))
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
BEGIN
    IF EXISTS (SELECT 1 FROM competition.players WHERE game_id = p_game_id AND id <> p_id AND is_deleted = false) THEN
        RETURN QUERY SELECT 2, 'El ID de juego ya está en uso.'::VARCHAR(200);
        RETURN;
    END IF;

    UPDATE competition.players
    SET nickname = p_nickname, game_id = p_game_id, state_id = p_state_id, updated_at = now()
    WHERE id = p_id;

    RETURN QUERY SELECT 0, 'Datos de jugador actualizados.'::VARCHAR(200);
EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT 99, SQLERRM::VARCHAR(200);
END;
$$;

CREATE OR REPLACE FUNCTION competition.fn_players_soft_delete(p_id INT)
RETURNS TABLE(error_code INT, error_message VARCHAR(200))
LANGUAGE plpgsql
SET search_path = ''
AS $$
#variable_conflict use_column
BEGIN
    UPDATE competition.players
    SET is_deleted = true, deleted_at = now(), state_id = 2
    WHERE id = p_id;

    RETURN QUERY SELECT 0, 'Jugador eliminado.'::VARCHAR(200);
EXCEPTION WHEN OTHERS THEN
    RETURN QUERY SELECT 99, SQLERRM::VARCHAR(200);
END;
$$;

CREATE OR REPLACE FUNCTION competition.fn_players_get_select()
RETURNS TABLE(id INT, nickname VARCHAR(75))
LANGUAGE sql
SET search_path = ''
AS $$
    SELECT p.id, CONCAT(p.nickname, ' (', p.game_id, ')')
    FROM competition.players p
    WHERE p.is_deleted = false
      AND p.player_state_id = 2
      AND p.state_id = 1
    ORDER BY p.nickname;
$$;
