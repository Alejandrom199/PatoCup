-- ══════════════════════════════════════════════════════════════════════════
-- PatoCup — Funciones del schema audit
-- ══════════════════════════════════════════════════════════════════════════

CREATE OR REPLACE FUNCTION audit.fn_system_logs_create(
    p_action_type VARCHAR(50), p_message TEXT, p_ip_address VARCHAR(50), p_user_id INT DEFAULT NULL
)
RETURNS VOID
LANGUAGE sql
SET search_path = ''
AS $$
    INSERT INTO audit.system_logs (user_id, action_type, message, ip_address, created_at)
    VALUES (p_user_id, p_action_type, p_message, p_ip_address, now());
$$;

CREATE OR REPLACE FUNCTION audit.fn_system_logs_get_all(p_page_number INT DEFAULT 1, p_page_size INT DEFAULT 20)
RETURNS TABLE(
    id INT, user_id INT, username VARCHAR(50), action_type VARCHAR(50), message TEXT,
    ip_address VARCHAR(50), created_at TIMESTAMPTZ, total_records BIGINT
)
LANGUAGE sql
SET search_path = ''
AS $$
    SELECT
        l.id, l.user_id, u.username, l.action_type, l.message, l.ip_address, l.created_at,
        COUNT(*) OVER() AS total_records
    FROM audit.system_logs l
    LEFT JOIN security.users u ON l.user_id = u.id
    ORDER BY l.created_at DESC
    LIMIT p_page_size OFFSET (p_page_number - 1) * p_page_size;
$$;
