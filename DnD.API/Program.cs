using DnD.API.API.Filtros;
using DnD.API.Aplicacao.Servicos;
using DnD.API.Dominio.Repositorios;
using DnD.API.Hubs;
using DnD.API.Infraestrutura.Dados;
using DnD.API.Infraestrutura.Repositorios;
using DnD.API.Infraestrutura.Servicos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IO.Compression;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Limites do Kestrel ──────────────────────────────────────────────────────────
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxConcurrentConnections = 200;
    options.Limits.MaxConcurrentUpgradedConnections = 200; // WebSockets (SignalR)
    options.Limits.MaxRequestBodySize = 20 * 1024 * 1024; // 20MB (upload de imagens)
});

// ── Logging persistente (Serilog) ──────────────────────────────────────────────
builder.Host.UseSerilog((context, services, loggerConfig) => loggerConfig
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(AppContext.BaseDirectory, "logs", "dnd-api-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        shared: true));

// ── Banco de dados ────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql =>
        {
            npgsql.CommandTimeout(15);
            npgsql.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
        }));

// ── Inicialização do banco em background (não bloqueia o startup) ─────────────
builder.Services.AddHostedService<InicializadorBancoDados>();

// ── Repositórios ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IVendedorRepositorio,         VendedorRepositorio>();
builder.Services.AddScoped<ICompradorRepositorio,        CompradorRepositorio>();
builder.Services.AddScoped<IProdutoRepositorio,          ProdutoRepositorio>();
builder.Services.AddScoped<IPedidoRepositorio,           PedidoRepositorio>();
builder.Services.AddScoped<IMensagemRepositorio,         MensagemRepositorio>();
builder.Services.AddScoped<ICategoriaRepositorio,        CategoriaRepositorio>();
builder.Services.AddScoped<IConfiguracaoLojaRepositorio, ConfiguracaoLojaRepositorio>();

// ── Serviços de infraestrutura ────────────────────────────────────────────────
builder.Services.AddScoped<IJwtServico,           JwtServico>();
builder.Services.AddScoped<IArmazenamentoServico, ArmazenamentoServico>();
builder.Services.AddHttpClient();

// ── Serviços de aplicação ─────────────────────────────────────────────────────
builder.Services.AddScoped<AutenticacaoServico>();
builder.Services.AddScoped<ProdutoServico>();
builder.Services.AddScoped<PedidoServico>();
builder.Services.AddScoped<AdminServico>();
builder.Services.AddScoped<DashboardServico>();
builder.Services.AddScoped<MensagemServico>();
builder.Services.AddScoped<ConfiguracaoLojaServico>();
builder.Services.AddScoped<CategoriaServico>();

// ── Autenticação JWT ──────────────────────────────────────────────────────────
var jwtKey = builder.Configuration["JwtSettings:SecretKey"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer      = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience    = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

// ── Compressão de resposta (Brotli/Gzip) ────────────────────────────────────────
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);

builder.Services.AddHealthChecks();
builder.Services.AddAuthorization();
builder.Services.AddSignalR();
builder.Services.AddControllers(options =>
    options.Filters.Add<FiltroExcecoesDominio>());

var allowedOrigins = builder.Configuration["AllowedOrigins"]?.Split(',')
    ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));

// ── Pipeline ──────────────────────────────────────────────────────────────────
var app = builder.Build();

// Captura qualquer exceção não tratada (fora do domínio) para que o processo
// nunca derrube a requisição sem log — e o cliente recebe um erro genérico, não
// uma página branca/stack trace.
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        if (feature?.Error is { } ex)
        {
            Log.Error(ex, "Erro não tratado em {Path}", context.Request.Path);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new { message = "Ocorreu um erro interno. Tente novamente em instantes." });
    });
});

app.UseSerilogRequestLogging();

app.UseResponseCompression();
app.UseStaticFiles();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

try
{
    Log.Information("DnD.API iniciando...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "DnD.API encerrado inesperadamente durante a inicialização");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
