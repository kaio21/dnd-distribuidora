using DnD.API.API.Filtros;
using DnD.API.Aplicacao.Servicos;
using DnD.API.Dominio.Repositorios;
using DnD.API.Hubs;
using DnD.API.Infraestrutura.Dados;
using DnD.API.Infraestrutura.Repositorios;
using DnD.API.Infraestrutura.Servicos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Banco de dados ────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql =>
        {
            npgsql.CommandTimeout(30);
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

app.UseStaticFiles();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
