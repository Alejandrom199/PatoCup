using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PatoCup.Application.Validators;
using PatoCup.Application.Validators.Audit;
using PatoCup.Application.Validators.Competition;
using PatoCup.Application.Wrappers;
using System.Reflection;

namespace PatoCup.WebAPI.Extensions
{
    public static class ValidationExtensions
    {
        public static void AddValidationConfig(this IServiceCollection services)
        {
            // Habilita la validación automática para los Controladores
            services.AddFluentValidationAutoValidation();

            // Registra los validadores específicos
            services.AddValidatorsFromAssemblyContaining<CreateAuditLogValidator>();

            services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

            services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();
            services.AddValidatorsFromAssemblyContaining<UpdateUserValidator>();

            services.AddValidatorsFromAssemblyContaining<CreateTournamentValidator>();
            services.AddValidatorsFromAssemblyContaining<UpdateTournamentValidator>();

            services.AddValidatorsFromAssemblyContaining<CreatePhaseValidator>();
            services.AddValidatorsFromAssemblyContaining<UpdatePhaseValidator>();

            services.AddValidatorsFromAssemblyContaining<CreateMatchValidator>();
            services.AddValidatorsFromAssemblyContaining<RegisterMatchResultValidator>();

            services.AddValidatorsFromAssemblyContaining<PublicSubmitPlayerValidator>();
            services.AddValidatorsFromAssemblyContaining<UpdatePlayerValidator>();

            services.AddValidatorsFromAssemblyContaining<PaginationFilterValidator>();

            // Configura la respuesta personalizada para errores de validación
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var firstError = context.ModelState
                        .OrderByDescending(x => x.Key)
                        .SelectMany(v => v.Value!.Errors)
                        .Select(e => e.ErrorMessage)
                        .FirstOrDefault();

                    var response = new Response<string>(firstError ?? "Error de validación");

                    return new BadRequestObjectResult(response);
                };
            });
        }
    }
}