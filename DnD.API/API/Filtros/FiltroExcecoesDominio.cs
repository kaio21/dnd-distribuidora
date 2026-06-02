using DnD.API.Dominio;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DnD.API.API.Filtros;

public class FiltroExcecoesDominio : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not DomainException ex)
            return;

        var statusCode = ex switch
        {
            CredenciaisInvalidasException => StatusCodes.Status401Unauthorized,
            ConflitoDeDadosException      => StatusCodes.Status409Conflict,
            _                             => StatusCodes.Status400BadRequest
        };

        context.Result = new ObjectResult(new { message = ex.Message })
        {
            StatusCode = statusCode
        };

        context.ExceptionHandled = true;
    }
}
