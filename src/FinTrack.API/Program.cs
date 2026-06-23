using FinTrack.API.Middleware;
using FinTrack.API.Services;
using FinTrack.Application.Common.Behaviours;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using FinTrack.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ──────────────────────────────────────────────────────────────────
// Configure Serilog before anything else so startup errors are captured too.
// ReadFromConfiguration reads sink/level settings from appsettings.json,
// so you can change log levels without redeploying.
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// ── Infrastructure ────────────────────────────────────────────────────────────
// Single call registers: DbContext, all repositories, UnitOfWork,
// ITokenEncryptionService. Everything Infrastructure owns in one place.
builder.Services.AddInfrastructure(builder.Configuration);

// ── Application ───────────────────────────────────────────────────────────────
// MediatR scans the Application assembly for every IRequestHandler,
// registering each handler with the DI container automatically.
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(FinTrack.Application.Common.Models.Result).Assembly));

// Pipeline behaviours — order matters:
// LoggingBehaviour outermost (registered first) so it captures timing
// even on validation failures. ValidationBehaviour runs second,
// short-circuiting before the real handler if validation fails.
builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(LoggingBehaviour<,>));

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehaviour<,>));

// Scan Application assembly for every AbstractValidator<T> and register it.
builder.Services.AddValidatorsFromAssembly(
    typeof(FinTrack.Application.Common.Models.Result).Assembly);

// ── API layer services ────────────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// ── Authentication ────────────────────────────────────────────────────────────
// JWT Bearer auth — validates every incoming token against our secret key.
// The secret and issuer come from configuration, never from code.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"]
                    ?? throw new InvalidOperationException("Jwt:Key is missing from configuration.")))
        };
    });

builder.Services.AddAuthorization();

// ── Controllers + Swagger ─────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── Health checks ──────────────────────────────────────────────────────────────
// /health endpoint returns 200 if the API is running. We'll add a real
// database reachability check here in a follow-up PR.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<FinTrackDbContext>();

var app = builder.Build();

// ── Middleware pipeline — order is critical ───────────────────────────────────
// ExceptionHandlingMiddleware must be first so it wraps everything else —
// exceptions from authentication, routing, controllers, all caught here.
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Authentication before Authorization — you must know who someone is
// before you can decide what they're allowed to do.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health");

app.Run();