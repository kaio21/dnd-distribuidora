using DnD.API.Aplicacao.DTOs;
using DnD.API.Dominio;
using DnD.API.Dominio.Entidades;
using DnD.API.Dominio.Repositorios;

namespace DnD.API.Aplicacao.Servicos;

public class AutenticacaoServico
{
    private readonly IVendedorRepositorio _vendedorRepo;
    private readonly ICompradorRepositorio _compradorRepo;
    private readonly IJwtServico _jwt;

    public AutenticacaoServico(
        IVendedorRepositorio vendedorRepo,
        ICompradorRepositorio compradorRepo,
        IJwtServico jwt)
    {
        _vendedorRepo = vendedorRepo;
        _compradorRepo = compradorRepo;
        _jwt = jwt;
    }

    public async Task<RespostaAutenticacaoDto> LoginVendedorAsync(LoginVendedorDto dto)
    {
        var vendedor = await _vendedorRepo.ObterPorEmailAsync(dto.Email)
            ?? throw new CredenciaisInvalidasException("Email ou senha inválidos.");

        if (!vendedor.ValidarSenha(dto.Senha))
            throw new CredenciaisInvalidasException("Email ou senha inválidos.");

        if (!vendedor.Aprovado)
            throw new CredenciaisInvalidasException("Conta ainda não aprovada. Aguarde o administrador.");

        var token = _jwt.GerarToken(vendedor.Id, vendedor.Email, "Seller", vendedor.Nome, vendedor.Perfil);
        return new RespostaAutenticacaoDto(token, "Seller", vendedor.Id, vendedor.Nome, vendedor.Email,
            vendedor.Perfil == "Admin", vendedor.Perfil);
    }

    public async Task<RespostaAutenticacaoDto> RegistrarCompradorAsync(RegistrarCompradorDto dto)
    {
        var cpf  = string.IsNullOrWhiteSpace(dto.CPF)  ? null : dto.CPF;
        var cnpj = string.IsNullOrWhiteSpace(dto.CNPJ) ? null : dto.CNPJ;

        var duplicado = await _compradorRepo.ExisteComEmailCpfOuCnpjAsync(dto.Email, cpf, cnpj);
        if (duplicado)
            throw new ConflitoDeDadosException("Email, CPF ou CNPJ já cadastrado.");

        int? indicadoPorId = null;
        if (dto.IndicadoPorVendedorId.HasValue &&
            await _vendedorRepo.ExisteAprovadoPorIdAsync(dto.IndicadoPorVendedorId.Value))
        {
            indicadoPorId = dto.IndicadoPorVendedorId;
        }

        var senhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha);
        var comprador = Comprador.Criar(dto.Nome, dto.NomeLoja, dto.Email, senhaHash,
            dto.Telefone, dto.Endereco, cpf, cnpj, indicadoPorId);

        await _compradorRepo.AdicionarAsync(comprador);
        await _compradorRepo.SalvarAlteracoesAsync();

        var token = _jwt.GerarToken(comprador.Id, comprador.Email, "Buyer", comprador.Nome);
        return new RespostaAutenticacaoDto(token, "Buyer", comprador.Id, comprador.Nome, comprador.Email);
    }

    public async Task<RespostaAutenticacaoDto> LoginCompradorAsync(LoginCompradorDto dto)
    {
        var comprador = await _compradorRepo.ObterPorEmailAsync(dto.Email)
            ?? throw new CredenciaisInvalidasException("Email ou senha inválidos.");

        if (!comprador.ValidarSenha(dto.Senha))
            throw new CredenciaisInvalidasException("Email ou senha inválidos.");

        var token = _jwt.GerarToken(comprador.Id, comprador.Email, "Buyer", comprador.Nome);
        return new RespostaAutenticacaoDto(token, "Buyer", comprador.Id, comprador.Nome, comprador.Email);
    }
}
