using FluentValidation;
using InstantSyncBackend.Application.Interfaces.IRepositories;
using InstantSyncBackend.Application.Interfaces.IServices;
using InstantSyncBackend.Application.Models;
using InstantSyncBackend.Application.Validators;
using InstantSyncBackend.Domain.Entities;
using InstantSyncBackend.Persistence.DbContext;
using InstantSyncBackend.Persistence.Repositories;
using InstantSyncBackend.Persistence.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

namespace InstantSyncBackend.Persistence.ServiceCollections;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Serilog
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: "Logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        services.AddLogging(loggingBuilder =>
            loggingBuilder.AddSerilog(logger, dispose: true));

        // DbContext with PostgreSQL
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
            ));

        // Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // JWT Authentication
        var jwtKey = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]);
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(jwtKey)
            };
        });

        // Configure CORS
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
        });

        // Register repositories
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        // Register services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.AddScoped<IEmailService, EmailService>();

        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();

        return services;
    }
}
