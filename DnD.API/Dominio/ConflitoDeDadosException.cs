namespace DnD.API.Dominio;

public class ConflitoDeDadosException : DomainException
{
    public ConflitoDeDadosException(string mensagem) : base(mensagem) { }
}
