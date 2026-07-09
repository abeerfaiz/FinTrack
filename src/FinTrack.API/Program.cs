using FinTrack.API.Middleware;
using FinTrack.API.Services;
using FinTrack.Application.Common.Behaviours;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Infrastructure;
using FinTrack.Infrastructure.BackgroundJobs;
using FinTrack.Infrastructure.Persistence;
using FluentValidation;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using System.Text;

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
builder.Services.AddSwaggerGen(options =>
{
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
    };

    options.AddSecurityDefinition("Bearer", securityScheme);

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", document), new List<string>() }
    });
});

// ── Health checks ──────────────────────────────────────────────────────────────
// /health endpoint returns 200 if the API is running. We'll add a real
// database reachability check here in a follow-up PR.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<FinTrackDbContext>();


// Rate limiting — protects auth endpoints from brute force attacks.
// Fixed window: max 5 requests per minute per IP address on auth endpoints.
// After 5 failed attempts in a minute, subsequent requests get 429 Too Many Requests.
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", config =>
    {
        config.PermitLimit = 5;
        config.Window = TimeSpan.FromMinutes(1);
        config.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 0;
    });

    options.RejectionStatusCode = 429;
});

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
app.UseRateLimiter(); // after auth so authenticated requests are still rate limited

// Hangfire dashboard — visual job monitoring at /hangfire
// Restricted to development environment — never expose in production
// without proper authentication.
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        // In development, allow unauthenticated access to the dashboard.
        // In production, replace with a proper authorization filter.
        Authorization = new[] { new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter() }
    });
}

app.MapControllers();
app.MapHealthChecks("/health");

// Register recurring jobs after the app is fully built and
// DI is configured. Must be called after app.Build() —
// RecurringJob.AddOrUpdate requires Hangfire's storage to be
// initialised first.
JobScheduler.RegisterRecurringJobs(
    app.Services.GetRequiredService<ILogger<Program>>());

app.Run();