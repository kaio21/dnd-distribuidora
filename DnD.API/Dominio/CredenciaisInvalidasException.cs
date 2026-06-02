namespace DnD.API.Dominio;

public class CredenciaisInvalidasException : DomainException
{
    public CredenciaisInvalidasException(string mensagem) : base(mensagem) { }
}
