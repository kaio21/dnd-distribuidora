using DnD.API.Aplicacao.DTOs;
using DnD.API.Dominio.Entidades;
using DnD.API.Dominio.Repositorios;

namespace DnD.API.Aplicacao.Servicos;

public class ConfiguracaoLojaServico
{
    private readonly IConfiguracaoLojaRepositorio _configRepo;

    public ConfiguracaoLojaServico(IConfiguracaoLojaRepositorio configRepo) =>
        _configRepo = configRepo;

    public async Task<ConfiguracaoLojaDto> ObterAsync()
    {
        var config = await _configRepo.ObterAsync() ?? ConfiguracaoLoja.Criar();
        return MapearParaDto(config);
    }

    public async Task<ConfiguracaoLojaDto> AtualizarAsync(ConfiguracaoLojaDto dto)
    {
        var config = await _configRepo.ObterAsync();

        if (config is null)
        {
            config = ConfiguracaoLoja.Criar();
            await _configRepo.AdicionarAsync(config);
        }

        config.Atualizar(dto.HoraAbertura, dto.HoraFechamento, dto.Aberta, dto.NumeroWhatsApp);
        await _configRepo.SalvarAlteracoesAsync();

        return MapearParaDto(config);
    }

    private static ConfiguracaoLojaDto MapearParaDto(ConfiguracaoLoja c) =>
        new(c.HoraAbertura, c.HoraFechamento, c.Aberta, c.NumeroWhatsApp);
}
