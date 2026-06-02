using DnD.API.Dominio.Entidades;
using DnD.API.Dominio.Repositorios;
using DnD.API.Infraestrutura.Dados;
using Microsoft.EntityFrameworkCore;

namespace DnD.API.Infraestrutura.Repositorios;

public class ConfiguracaoLojaRepositorio : IConfiguracaoLojaRepositorio
{
    private readonly AppDbContext _db;

    public ConfiguracaoLojaRepositorio(AppDbContext db) => _db = db;

    public Task<ConfiguracaoLoja?> ObterAsync() =>
        _db.ConfiguracoesLoja.FirstOrDefaultAsync();

    public async Task AdicionarAsync(ConfiguracaoLoja configuracao) =>
        await _db.ConfiguracoesLoja.AddAsync(configuracao);

    public Task SalvarAlteracoesAsync() =>
        _db.SaveChangesAsync();
}
