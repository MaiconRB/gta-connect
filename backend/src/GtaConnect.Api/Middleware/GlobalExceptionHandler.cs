using GtaConnect.Application.Resources;
using GtaConnect.Domain.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace GtaConnect.Api.Middleware;

/// <summary>
/// Ponto único de tradução de exceções para respostas HTTP no formato ProblemDetails (RFC 7807).
/// Controllers não precisam de try/catch — exceções sobem até aqui e viram uma resposta consistente,
/// já no idioma da requisição (RequestLocalizationMiddleware define a cultura antes deste handler rodar).
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger,
        IStringLocalizer<SharedResource> localizer)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
        _localizer = localizer;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title, extensions) = MapException(exception);

        httpContext.Response.StatusCode = statusCode;

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Erro não tratado ao processar {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        }

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Extensions = extensions,
            },
        });
    }

    private (int StatusCode, string Title, Dictionary<string, object?> Extensions) MapException(Exception exception) =>
        exception switch
        {
            ValidationAppException validationException => (
                StatusCodes.Status400BadRequest,
                _localizer["Validation_Title"],
                new Dictionary<string, object?> { ["errors"] = validationException.Errors }),

            NotFoundException notFoundException => (
                StatusCodes.Status404NotFound,
                _localizer["NotFound_Message", notFoundException.EntityName ?? string.Empty, notFoundException.Key ?? string.Empty],
                []),

            _ => (
                StatusCodes.Status500InternalServerError,
                _localizer["UnexpectedError_Title"],
                []),
        };
}
