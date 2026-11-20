using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using LabTrack.Api.Controllers;
using LabTrack.Api.Extensions;
using LabTrack.Api.Filters;
using LabTrack.Api.Health;
using LabTrack.Api.Hubs;
using LabTrack.Api.Middleware;
using LabTrack.Api.Services;
using LabTrack.Api.Workers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidateModelAttribute>();
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApplicationServices();

builder.Services.AddScoped<AuditContext>();

builder.Services.AddScoped<LabTrack.Api.Controllers.IUserService, LabTrack.Api.Services.UserService>();
builder.Services.AddScoped<LabTrack.Api.Controllers.IAnalyticsService, LabTrack.Api.Services.AnalyticsService>();

builder.Services.AddLabTrackSwagger();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSignalR();

builder.Services.AddHttpClient<PythonAiHealthCheck>();

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database_check", tags: new[] { "db", "ready" })
    .AddCheck<PythonAiHealthCheck>("python_ai_check", tags: new[] { "ai", "ready" })
    .AddCheck<ReadinessHealthCheck>("readiness_check", tags: new[] { "readiness" });

builder.Services.AddHostedService<EmailDispatchWorker>();
builder.Services.AddHostedService<NotificationWorker>();
builder.Services.AddHostedService<PcSessionHeartbeatWorker>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("LabTrackCorsPolicy", policy =>
    {
        policy.SetIsOriginAllowed(origin => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

builder.Services.AddDbContext<LabTrack.Infrastructure.Persistence.DbContext.LabTrackDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<
    LabTrack.Application.Interfaces.IUnitOfWork,
    LabTrack.Infrastructure.Persistence.Repositories.UnitOfWork>();

builder.Services.AddScoped<
    LabTrack.Application.Interfaces.IPasswordHasher,
    LabTrack.Infrastructure.Security.PasswordHasher>();

builder.Services.AddScoped<
    LabTrack.Application.Interfaces.IPythonAiClient,
    LabTrack.Infrastructure.Ai.PythonAiClient>();

builder.Services.AddHttpClient<LabTrack.Api.Services.PythonAiService>(client =>
{
    var baseUrl = builder.Configuration["PythonAi:BaseUrl"]
        ?? builder.Configuration["AiEngine:BaseUrl"]
        ?? builder.Configuration["PythonAiUrl"]
        ?? builder.Configuration["PythonBaseUrl"]
        ?? "http://127.0.0.1:8000";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(60);
});

builder.Services.AddScoped<LabTrack.Api.Models.IPythonAiClient>(sp =>
    sp.GetRequiredService<LabTrack.Api.Services.PythonAiService>());

builder.Services.AddScoped<
    LabTrack.Application.Interfaces.IQrGeneratorEngine,
    LabTrack.Infrastructure.Qr.QrGeneratorEngine>();

builder.Services.AddScoped<
    LabTrack.Application.Interfaces.IJwtTokenGenerator,
    LabTrack.Infrastructure.Security.JwtTokenGenerator>();

builder.Services.AddScoped<
    LabTrack.Application.Interfaces.IQrTokenService,
    LabTrack.Infrastructure.Qr.QrTokenService>();

builder.Services.AddScoped<
    LabTrack.Api.Controllers.ILaboratoriesService,
    LabTrack.Api.Services.LaboratoriesService>();

builder.Services.AddScoped<
    LabTrack.Api.Controllers.IAssetService,
    LabTrack.Api.Services.AssetService>();

builder.Services.AddScoped<
    LabTrack.Api.Controllers.INotificationService,
    LabTrack.Api.Services.NotificationService>();

builder.Services.AddScoped<
    LabTrack.Api.Controllers.IReservationService,
    LabTrack.Api.Services.ReservationService>();

builder.Services.AddScoped<
    LabTrack.Api.Controllers.IProblemReportService,
    LabTrack.Api.Services.ProblemReportService>();

builder.Services.AddScoped<
    LabTrack.Api.Controllers.IRepairTicketService,
    LabTrack.Api.Services.RepairTicketService>();

builder.Services.AddScoped<
    LabTrack.Api.Controllers.IMaintenanceService,
    LabTrack.Api.Services.MaintenanceService>();

builder.Services.AddScoped<
    LabTrack.Api.Controllers.IAuditLogService,
    LabTrack.Api.Services.AuditLogService>();

builder.Services.AddScoped<
    LabTrack.Api.Controllers.ICommunicationService,
    LabTrack.Api.Services.CommunicationService>();

builder.Services.AddScoped<
    LabTrack.Api.Controllers.IFinancialService,
    LabTrack.Api.Services.FinancialService>();

builder.Services.AddScoped<
    LabTrack.Api.Controllers.INetworkService,
    LabTrack.Api.Services.NetworkService>();

builder.Services.AddScoped<
    LabTrack.Api.Controllers.IQrService,
    LabTrack.Api.Services.QrService>();

builder.Services.AddScoped<
    LabTrack.Api.Controllers.IReplacementService,
    LabTrack.Api.Services.ReplacementService>();

builder.Services.AddScoped<
    LabTrack.Api.Controllers.IPcUsageService,
    LabTrack.Api.Services.PcUsageService>();

builder.Services.AddAuthentication("Bearer")
    .AddScheme<AuthenticationSchemeOptions, LabTrackAuthHandler>("Bearer", null);

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<RequestIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseLabTrackSwagger();
}

app.UseHttpsRedirection();
app.UseCors("LabTrackCorsPolicy");
app.UseRouting();

app.UseAuthentication();
app.UseMiddleware<JwtRoleMiddleware>();
app.UseMiddleware<AuditContextMiddleware>();
app.UseMiddleware<LabTrack.Api.Middleware.AuditLogMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.MapHub<LabTrackHub>("/hubs/labtrack");
app.MapHub<AiChatHub>("/hubs/aichat");
app.MapHub<SessionHub>("/hubs/session");

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = WriteHealthResponseAsync
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready") || check.Tags.Contains("readiness"),
    ResponseWriter = WriteHealthResponseAsync
});

using (var scope = app.Services.CreateScope())
{
    var connString = builder.Configuration.GetConnectionString("DefaultConnection");
    var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
    var healthCheckService = scope.ServiceProvider.GetRequiredService<HealthCheckService>();
    var actionProvider = scope.ServiceProvider.GetRequiredService<IActionDescriptorCollectionProvider>();
    var hostedServices = scope.ServiceProvider.GetServices<IHostedService>()
        .Where(s => s.GetType().Namespace?.StartsWith("LabTrack") == true)
        .Select(s => s.GetType().Name)
        .ToList();

    var aiBaseUrl = builder.Configuration["AiEngine:BaseUrl"]
        ?? builder.Configuration["PythonAi:BaseUrl"]
        ?? "http://127.0.0.1:8000";

    bool dbOnline = false;
    string dbEndpoint = "N/A";
    string dbVersion = "N/A";
    long totalTables = 0;
    long totalSchemas = 0;
    string pgVectorVersion = "Not Installed";
    long dbLatencyMs = 0;

    try
    {
        var dbSw = Stopwatch.StartNew();
        await using var conn = new NpgsqlConnection(connString);
        await conn.OpenAsync();
        dbSw.Stop();
        dbLatencyMs = dbSw.ElapsedMilliseconds;

        dbOnline = true;
        dbEndpoint = $"{conn.Database} @ {conn.Host}:{conn.Port}";
        dbVersion = $"PostgreSQL {conn.ServerVersion}";

        await using (var cmd = new NpgsqlCommand(@"
            SELECT COUNT(*), COUNT(DISTINCT table_schema)
            FROM information_schema.tables
            WHERE table_type = 'BASE TABLE'
              AND table_schema NOT IN ('pg_catalog', 'information_schema');", conn))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                totalTables = reader.GetInt64(0);
                totalSchemas = reader.GetInt64(1);
            }
        }

        await using (var vecCmd = new NpgsqlCommand("SELECT extversion FROM pg_extension WHERE extname = 'vector' LIMIT 1;", conn))
        {
            var extVer = await vecCmd.ExecuteScalarAsync();
            if (extVer != null) pgVectorVersion = $"pgvector v{extVer}";
        }
    }
    catch (Exception ex)
    {
        dbEndpoint = $"ERROR: {ex.Message}";
    }

    bool aiOnline = false;
    string aiStatusText = "OFFLINE";
    string aiModel = "N/A";
    int aiTables = 0;
    int aiVectors = 0;
    long aiLatencyMs = 0;

    try
    {
        using var httpClient = httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(5);
        var aiSw = Stopwatch.StartNew();
        using var response = await httpClient.GetAsync($"{aiBaseUrl.TrimEnd('/')}/api/v1/ai/health");
        aiSw.Stop();
        aiLatencyMs = aiSw.ElapsedMilliseconds;

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var rawStatus = root.TryGetProperty("status", out var sProp) ? sProp.GetString() : "ok";
            aiModel = root.TryGetProperty("llm", out var mProp) ? mProp.GetString() ?? "unknown" : "unknown";
            aiTables = root.TryGetProperty("tables_discovered", out var tProp) && tProp.TryGetInt32(out var tv) ? tv : 0;
            aiVectors = root.TryGetProperty("vector_memories_stored", out var vProp) && vProp.TryGetInt32(out var vv) ? vv : 0;

            aiOnline = true;
            aiStatusText = $"ONLINE ({rawStatus?.ToUpperInvariant()} in {aiLatencyMs}ms)";
        }
        else
        {
            aiStatusText = $"DEGRADED (HTTP {(int)response.StatusCode})";
        }
    }
    catch (Exception ex)
    {
        aiStatusText = $"OFFLINE ({ex.Message})";
    }

    var healthReport = await healthCheckService.CheckHealthAsync();
    int healthyChecks = healthReport.Entries.Count(e => e.Value.Status == HealthStatus.Healthy);
    int totalChecks = healthReport.Entries.Count;

    var controllerActions = actionProvider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>().ToList();
    int controllerCount = controllerActions.Select(a => a.ControllerName).Distinct().Count();
    int endpointCount = controllerActions.Count;

    app.Lifetime.ApplicationStarted.Register(() =>
    {
        Console.OutputEncoding = Encoding.UTF8;
        const int innerWidth = 84;
        const char boxTopLeft = '\u2554';
        const char boxTopRight = '\u2557';
        const char boxBotLeft = '\u255A';
        const char boxBotRight = '\u255D';
        const char boxHoriz = '\u2550';
        const string boxVert = "\u2551";
        const string boxSecLeft = "\u2560\u2550\u2550";
        const char boxSecRight = '\u2563';

        void WriteBorder(char left, char fill, char right, ConsoleColor color = ConsoleColor.Cyan)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"{left}{new string(fill, innerWidth)}{right}");
            Console.ResetColor();
        }

        void WriteCentered(string text, ConsoleColor textColor = ConsoleColor.White, ConsoleColor borderColor = ConsoleColor.Cyan)
        {
            if (text.Length > innerWidth) text = text[..innerWidth];
            int leftPad = (innerWidth - text.Length) / 2;
            int rightPad = innerWidth - text.Length - leftPad;

            Console.ForegroundColor = borderColor;
            Console.Write(boxVert);
            Console.ForegroundColor = textColor;
            Console.Write($"{new string(' ', leftPad)}{text}{new string(' ', rightPad)}");
            Console.ForegroundColor = borderColor;
            Console.WriteLine(boxVert);
            Console.ResetColor();
        }

        void WriteSection(string title)
        {
            string header = $" [ {title} ] ";
            int rightFill = Math.Max(0, innerWidth - header.Length - 2);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(boxSecLeft);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(header);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{new string(boxHoriz, rightFill)}{boxSecRight}");
            Console.ResetColor();
        }

        void WriteRow(string label, string value, ConsoleColor valueColor = ConsoleColor.Green)
        {
            string leftPart = $"  {label,-20}: ";
            int maxValLen = innerWidth - leftPart.Length - 1;
            string cleanVal = value.Length > maxValLen ? value[..(maxValLen - 3)] + "..." : value;
            int padRight = innerWidth - leftPart.Length - cleanVal.Length;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(boxVert);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(leftPart);
            Console.ForegroundColor = valueColor;
            Console.Write(cleanVal);
            Console.Write(new string(' ', Math.Max(0, padRight)));
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(boxVert);
            Console.ResetColor();
        }

        var urls = app.Urls.Any() ? string.Join(", ", app.Urls) : "http://localhost:5000";

        Console.WriteLine();
        WriteBorder(boxTopLeft, boxHoriz, boxTopRight);
        WriteCentered("LABTRACK ENTERPRISE BACKEND ECOSYSTEM - LIVE DIAGNOSTICS", ConsoleColor.Cyan);
        WriteCentered($"Runtime: {RuntimeInformation.FrameworkDescription} | Env: {app.Environment.EnvironmentName}", ConsoleColor.DarkGray);

        WriteSection("1. C# CORE API SERVER & ROUTING");
        WriteRow("Listening URLs", urls, ConsoleColor.Green);
        WriteRow("Swagger UI", "http://localhost:5000/swagger", ConsoleColor.Cyan);
        WriteRow("REST API Routes", $"{controllerCount} Controllers / {endpointCount} Active Endpoints", ConsoleColor.White);
        WriteRow("SignalR Realtime", "3 Hubs (/hubs/labtrack, /hubs/aichat, /hubs/session)", ConsoleColor.White);
        WriteRow("Background Workers", $"{hostedServices.Count} Active ({string.Join(", ", hostedServices.Select(w => w.Replace("Worker", "")))})", ConsoleColor.White);

        WriteSection("2. POSTGRESQL DATABASE ENGINE (LIVE PROBE)");
        WriteRow("Connection Status", dbOnline ? $"ONLINE ({dbLatencyMs}ms ping)" : "OFFLINE", dbOnline ? ConsoleColor.Green : ConsoleColor.Red);
        WriteRow("Database Target", dbEndpoint, ConsoleColor.White);
        WriteRow("Server & Vector", $"{dbVersion} | {pgVectorVersion}", ConsoleColor.White);
        WriteRow("Discovered Schema", $"{totalTables} Base Tables across {totalSchemas} Schemas", ConsoleColor.Yellow);

        WriteSection("3. PYTHON FASTAPI AI MICROSERVICE (LIVE PROBE)");
        WriteRow("AI Engine Status", aiStatusText, aiOnline ? ConsoleColor.Green : ConsoleColor.Red);
        WriteRow("Service Endpoint", aiBaseUrl, ConsoleColor.White);
        WriteRow("Active LLM Model", aiModel, ConsoleColor.Magenta);
        WriteRow("RAG & Vector Sync", $"{aiTables} Tables Indexed | {aiVectors} Vector Memories Stored", ConsoleColor.Yellow);

        WriteSection("4. INTEGRATED HEALTH CHECK VERIFICATION");
        WriteRow("Overall Readiness", $"{healthReport.Status.ToString().ToUpperInvariant()} ({healthyChecks}/{totalChecks} Checks Passed in {healthReport.TotalDuration.TotalMilliseconds:F0}ms)",
            healthReport.Status == HealthStatus.Healthy ? ConsoleColor.Green : ConsoleColor.Red);

        foreach (var entry in healthReport.Entries)
        {
            var checkColor = entry.Value.Status == HealthStatus.Healthy ? ConsoleColor.Green : ConsoleColor.Red;
            WriteRow($"-> {entry.Key}", $"{entry.Value.Status} ({entry.Value.Duration.TotalMilliseconds:F0}ms) - {entry.Value.Description}", checkColor);
        }

        WriteBorder(boxBotLeft, boxHoriz, boxBotRight);
        Console.WriteLine();
    });
}

app.Run();

static Task WriteHealthResponseAsync(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";

    var response = new
    {
        status = report.Status.ToString(),
        totalDurationMilliseconds = report.TotalDuration.TotalMilliseconds,
        checks = report.Entries.Select(entry => new
        {
            name = entry.Key,
            status = entry.Value.Status.ToString(),
            description = entry.Value.Description,
            durationMilliseconds = entry.Value.Duration.TotalMilliseconds,
            exception = entry.Value.Exception?.Message
        })
    };

    var options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
}

public class AuditContext
{
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public string? UserRole { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? CorrelationId { get; set; }
}

public class LabTrackAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public LabTrackAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "00000000-0000-0000-0000-000000000001"),
            new Claim("sub", "00000000-0000-0000-0000-000000000001"),
            new Claim("userId", "00000000-0000-0000-0000-000000000001"),
            new Claim(ClaimTypes.Email, "technician@jrmsu.edu.ph"),
            new Claim(ClaimTypes.Name, "LabTrack Admin Technician"),
            new Claim(ClaimTypes.Role, "TECHNICIAN"),
            new Claim("role", "TECHNICIAN")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}