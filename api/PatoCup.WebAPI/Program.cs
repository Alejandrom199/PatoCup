using Microsoft.AspNetCore.HttpOverrides;
using PatoCup.Application.Interfaces.Services.Competition;
using PatoCup.WebAPI.Extensions;
using PatoCup.WebAPI.Middlewares;
using System.Text.Json;

// Las funciones Postgres devuelven columnas en snake_case (new_id, error_code,
// role_name...); esto le dice a Dapper que las mapee a las propiedades PascalCase
// de las entidades/DTOs sin tener que alias-ear cada columna a mano.
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoMapper(cfg => { }, typeof(ITournamentService).Assembly);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfig();

builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddValidationConfig();

var app = builder.Build();

app.UseMiddleware<ErrorHandlerMiddleware>();

var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};

forwardedOptions.KnownNetworks.Clear();
forwardedOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedOptions);

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Railway usa esto (healthcheckPath en railway.toml) para saber si el contenedor está listo.
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();