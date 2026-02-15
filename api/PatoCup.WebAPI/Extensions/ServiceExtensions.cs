using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PatoCup.Application.Interfaces.Services.Audit;
using PatoCup.Application.Interfaces.Services.Common;
using PatoCup.Application.Interfaces.Services.Competition;
using PatoCup.Application.Interfaces.Services.Security;
using PatoCup.Application.UseCases.Audit;
using PatoCup.Application.UseCases.Common;
using PatoCup.Application.UseCases.Competition;
using PatoCup.Application.UseCases.Security;
using PatoCup.Application.Validators;
using PatoCup.Domain.Interfaces.Repositories.Audit;
using PatoCup.Domain.Interfaces.Repositories.Common;
using PatoCup.Domain.Interfaces.Repositories.Competition;
using PatoCup.Domain.Interfaces.Repositories.Security;
using PatoCup.Infrastructure.Persistence;
using PatoCup.Infrastructure.Persistence.Repositories.Audit;
using PatoCup.Infrastructure.Persistence.Repositories.Common;
using PatoCup.Infrastructure.Persistence.Repositories.Competition;
using PatoCup.Infrastructure.Persistence.Repositories.Security;
using PatoCup.Infrastructure.Services.Security;
using System.Reflection;

namespace PatoCup.WebAPI.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular", policy =>
                {
                    policy.WithOrigins("http://localhost:4200")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            services.AddSingleton<DapperContext>();

            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtGenerator, JwtGenerator>();

            services.AddScoped<ICatalogRepository, CatalogRepository>();
            services.AddScoped<ICatalogService, CatalogService>();

            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IAuthService, AuthService>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();

            services.AddScoped<ITournamentRepository, TournamentRepository>();
            services.AddScoped<ITournamentService, TournamentService>();

            services.AddScoped<IPhaseRepository, PhaseRepository>();
            services.AddScoped<IPhaseService, PhaseService>();

            services.AddScoped<IPlayerRepository, PlayerRepository>();
            services.AddScoped<IPlayerService, PlayerService>();

            services.AddScoped<IMatchRepository, MatchRepository>();
            services.AddScoped<IMatchService, MatchService>();

            services.AddScoped<IAuditRepository, AuditRepository>();
            services.AddScoped<IAuditService, AuditService>();

            services.AddScoped<IMenuRepository, MenuRepository>();
            services.AddScoped<IMenuService, MenuService>();
        }
    }
}