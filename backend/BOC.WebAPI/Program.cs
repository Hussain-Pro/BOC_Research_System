using System.Text;
using System.Threading.RateLimiting;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using BOC.Application;
using BOC.Infrastructure;
using BOC.WebAPI.Middlewares;
using BOC.WebAPI.Hubs;
using BOC.WebAPI.Extensions;

// ─────────────────────────────────────────────────────────────────────────────
// Bootstrap logger — captures startup failures before appsettings is loaded
// ─────────────────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting BOC Research Evaluation System...");

    var builder = WebApplication.CreateBuilder(args);

    // ─────────────────────────────────────────────────────────────────────────
    // Serilog — read full configuration from appsettings
    // ─────────────────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Application", "BOC.ResearchSystem")
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);
    });

    // ─────────────────────────────────────────────────────────────────────────
    // Application & Infrastructure Services
    // ─────────────────────────────────────────────────────────────────────────
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // ─────────────────────────────────────────────────────────────────────────
    // Controllers & API Explorer
    // ─────────────────────────────────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApi();

    // ─────────────────────────────────────────────────────────────────────────
    // CORS — read allowed origins from appsettings CorsPolicy:AllowedOrigins
    // ─────────────────────────────────────────────────────────────────────────
    var allowedOrigins = builder.Configuration
        .GetSection("CorsPolicy:AllowedOrigins")
        .Get<string[]>() ?? new[] { "http://localhost:4200" };

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("BOC_SPA_Policy", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Required for SignalR + HttpOnly cookie refresh tokens
        });
    });

    // ─────────────────────────────────────────────────────────────────────────
    // JWT Authentication — symmetric key from appsettings Jwt:SecurityKey
    // ─────────────────────────────────────────────────────────────────────────
    var jwtKey = builder.Configuration["Jwt:SecurityKey"]
        ?? "BOC_Research_Evaluation_2026_SuperSecretKey_ChangeInProduction_MinLength32Chars!";
    var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"]   ?? "BOC.ResearchSystem",
            ValidAudience            = builder.Configuration["Jwt:Audience"] ?? "BOC.ResearchSystem.Client",
            IssuerSigningKey         = signingKey,
            ClockSkew                = TimeSpan.Zero  // Strict 15-minute expiry
        };

        // Allow JWT from query-string for SignalR WebSocket handshake
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization();

    // ─────────────────────────────────────────────────────────────────────────
    // SignalR — Redis backplane for multi-instance support
    // ─────────────────────────────────────────────────────────────────────────
    var signalRRedis = builder.Configuration["SignalR:RedisBackplane"];
    var enableDetailedErrors = builder.Configuration.GetValue<bool>("SignalR:EnableDetailedErrors");

    var signalRBuilder = builder.Services.AddSignalR(options =>
    {
        options.EnableDetailedErrors = enableDetailedErrors;
    });

    if (!string.IsNullOrWhiteSpace(signalRRedis) &&
        !signalRRedis.StartsWith("localhost") == false ||
        builder.Environment.IsProduction())
    {
        // Only attach Redis backplane if a real Redis server is configured
        try
        {
            signalRBuilder.AddStackExchangeRedis(signalRRedis!, options =>
            {
                options.Configuration.ChannelPrefix =
                    StackExchange.Redis.RedisChannel.Literal("BOC_SignalR");
            });
            Log.Information("SignalR Redis backplane configured: {Redis}", signalRRedis);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "SignalR Redis backplane failed — falling back to in-memory.");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Rate Limiting — AspNetCoreRateLimit (IP-based)
    // ─────────────────────────────────────────────────────────────────────────
    builder.Services.AddMemoryCache();
    builder.Services.Configure<IpRateLimitOptions>(options =>
    {
        var loginAttempts = builder.Configuration.GetValue<int>("RateLimit:LoginMaxAttempts", 5);
        var loginWindow   = builder.Configuration.GetValue<int>("RateLimit:LoginWindowMinutes", 15);
        var apiLimit      = builder.Configuration.GetValue<int>("RateLimit:ApiMaxCallsPerMinute", 100);

        options.EnableEndpointRateLimiting = true;
        options.StackBlockedRequests       = false;
        options.HttpStatusCode             = 429;
        options.RealIpHeader               = "X-Real-IP";
        options.ClientIdHeader             = "X-ClientId";
        options.GeneralRules = new List<RateLimitRule>
        {
            // Login endpoint — 5 attempts / 15 minutes
            new RateLimitRule
            {
                Endpoint = "POST:/api/auth/login",
                Period   = $"{loginWindow}m",
                Limit    = loginAttempts
            },
            // All other API endpoints — 100 calls / minute
            new RateLimitRule
            {
                Endpoint = "*",
                Period   = "1m",
                Limit    = apiLimit
            }
        };
    });
    builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
    builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
    builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
    builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
    builder.Services.AddInMemoryRateLimiting();

    // ─────────────────────────────────────────────────────────────────────────
    // Health Checks
    // ─────────────────────────────────────────────────────────────────────────
    var hcBuilder = builder.Services.AddHealthChecks();

    if (builder.Configuration.GetValue<bool>("HealthChecks:SqlServerEnabled", true))
    {
        hcBuilder.AddSqlServer(
            connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!,
            name: "sqlserver",
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "db", "sql" });
    }

    if (builder.Configuration.GetValue<bool>("HealthChecks:RedisEnabled", true))
    {
        var redisCs = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
        hcBuilder.AddRedis(
            redisConnectionString: redisCs,
            name: "redis",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "cache", "redis" });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HttpContext accessor (needed for audit log interceptor)
    // ─────────────────────────────────────────────────────────────────────────
    builder.Services.AddHttpContextAccessor();

    // ─────────────────────────────────────────────────────────────────────────
    // Build application
    // ─────────────────────────────────────────────────────────────────────────
    var app = builder.Build();

    // Execute Seeder
    using (var scope = app.Services.CreateScope()) {
        var seeder = scope.ServiceProvider.GetRequiredService<BOC.Infrastructure.Persistence.DatabaseSeeder>();
        seeder.SeedAsync().Wait();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Middleware pipeline (ORDER IS CRITICAL)
    // ─────────────────────────────────────────────────────────────────────────

    // 1. Global exception handler — must be first
    app.UseMiddleware<GlobalExceptionMiddleware>();

    // 2. Serilog request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost",   httpContext.Request.Host.Value ?? string.Empty);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent",     httpContext.Request.Headers.UserAgent.ToString());
            diagnosticContext.Set("ClientIp",      httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty);
        };
    });

    // 3. Rate limiting
    app.UseIpRateLimiting();

    // 4. OpenAPI (development only)
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    // 5. HTTPS redirection
    app.UseHttpsRedirection();

    // 6. Static files (if serving Angular dist in production)
    app.UseStaticFiles();

    // 7. CORS — must be before Auth
    app.UseCors("BOC_SPA_Policy");

    // 8. Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // 9. 2FA enforcement gate
    app.UseMiddleware<TwoFactorEnforcementMiddleware>();

    // ─────────────────────────────────────────────────────────────────────────
    // Endpoint Mappings
    // ─────────────────────────────────────────────────────────────────────────

    // Health check endpoint — /health
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate         = _ => true,
        ResponseWriter    = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status     = report.Status.ToString(),
                duration   = report.TotalDuration.TotalMilliseconds,
                timestamp  = DateTime.UtcNow,
                checks     = report.Entries.Select(e => new
                {
                    name     = e.Key,
                    status   = e.Value.Status.ToString(),
                    duration = e.Value.Duration.TotalMilliseconds,
                    error    = e.Value.Exception?.Message
                })
            });
            await context.Response.WriteAsync(result);
        }
    });

    // Health check — live (no dependency checks, just pod alive)
    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false
    });

    // Health check — ready (full dependency check)
    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("db") || check.Tags.Contains("cache")
    });

    // Controllers
    app.MapControllers();

    // SignalR Hubs
    app.MapHub<NotificationHub>("/hubs/notifications");
    app.MapHub<ChatHub>("/hubs/chat");

    Log.Information("BOC Research System started successfully. Environment: {Env}", app.Environment.EnvironmentName);

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "BOC Research System terminated unexpectedly.");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

return 0;

