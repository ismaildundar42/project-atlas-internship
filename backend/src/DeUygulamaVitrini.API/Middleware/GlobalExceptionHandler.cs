using DeUygulamaVitrini.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DeUygulamaVitrini.API.Middleware;

/// <summary>
/// ASP.NET Core 8/9 modern exception handling yaklaşımını kullanan global hata işleyicisi.
/// Beklenmeyen veya doğrulama hatalarında istemciye standart ProblemDetails yanıtı döner.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "İstek işlenirken bir hata oluştu: {Message}", exception.Message);

        int statusCode;
        string title;
        string detail;

        switch (exception)
        {
            case ArgumentException argEx:
                statusCode = StatusCodes.Status400BadRequest;
                title = "Geçersiz Parametre / İstek";
                detail = argEx.Message;
                break;
            case KeyNotFoundException notFoundEx:
                statusCode = StatusCodes.Status404NotFound;
                title = "Kaynak Bulunamadı";
                detail = notFoundEx.Message;
                break;
            case EmbeddingProviderUnavailableException:
                statusCode = StatusCodes.Status503ServiceUnavailable;
                title = "Anlamsal Arama Servisi Kullanılamıyor";
                detail = "Anlamsal arama servisi şu anda kullanılamıyor. Lütfen daha sonra tekrar deneyin.";
                break;
            case GenerationProviderUnavailableException:
                statusCode = StatusCodes.Status503ServiceUnavailable;
                title = "Proje Asistanı Kullanılamıyor";
                detail = "Proje Asistanı şu anda kullanılamıyor. Lütfen daha sonra tekrar deneyin.";
                break;
            default:
                statusCode = StatusCodes.Status500InternalServerError;
                title = "Sunucu Hatası";
                detail = "İşlem gerçekleştirilirken beklenmeyen bir hata oluştu.";
                break;
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
