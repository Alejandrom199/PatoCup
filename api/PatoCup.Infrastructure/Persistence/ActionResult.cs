namespace PatoCup.Infrastructure.Persistence
{
    // Fila devuelta por las funciones PL/pgSQL de "acción" (create/update/soft-delete/reactivate),
    // equivalente a los parámetros OUTPUT @NewId/@ErrorCode/@ErrorMessage que usaba SQL Server.
    // Dapper.DefaultTypeMap.MatchNamesWithUnderscores (activado en Program.cs) mapea
    // new_id/error_code/error_message -> NewId/ErrorCode/ErrorMessage automáticamente.
    internal sealed class ActionResult
    {
        public int NewId { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
