using Microsoft.EntityFrameworkCore;

namespace DnD.API.Infraestrutura.Dados;

public class InicializadorBancoDados : BackgroundService
{
    private readonly IServiceProvider _servicos;
    private readonly ILogger<InicializadorBancoDados> _logger;

    public InicializadorBancoDados(IServiceProvider servicos, ILogger<InicializadorBancoDados> logger)
    {
        _servicos = servicos;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Aguarda o app subir completamente antes de tentar conectar ao banco
        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);

        const int tentativasMaximas = 5;

        for (int tentativa = 1; tentativa <= tentativasMaximas; tentativa++)
        {
            if (stoppingToken.IsCancellationRequested) return;

            try
            {
                await using var escopo = _servicos.CreateAsyncScope();
                var db = escopo.ServiceProvider.GetRequiredService<AppDbContext>();

                _logger.LogInformation("Conectando ao banco de dados (tentativa {Tentativa}/{Max})...", tentativa, tentativasMaximas);

                await db.Database.EnsureCreatedAsync(stoppingToken);
                await DadosIniciais.PopularAsync(db);

                _logger.LogInformation("Banco de dados inicializado com sucesso.");
                return;
            }
            catch (Exception ex) when (tentativa < tentativasMaximas)
            {
                var espera = TimeSpan.FromSeconds(Math.Pow(2, tentativa)); // 2s, 4s, 8s, 16s
                _logger.LogWarning("Falha ao conectar ao banco (tentativa {Tentativa}): {Erro}. Aguardando {Espera}s...",
                    tentativa, ex.Message, espera.TotalSeconds);
                await Task.Delay(espera, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("Não foi possível inicializar o banco após {Max} tentativas: {Erro}", tentativasMaximas, ex.Message);
            }
        }
    }
}
