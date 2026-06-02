namespace DnD.API.Dominio;

public class DomainException : Exception
{
    public DomainException(string mensagem) : base(mensagem) { }
}
