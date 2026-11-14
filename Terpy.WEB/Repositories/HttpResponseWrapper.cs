using System.Net;

namespace Terpy.WEB.Repositories;

public class HttpResponseWrapper<T>
{
    public HttpResponseWrapper(T? response, bool error, HttpResponseMessage httpResponseMessage)
    {
        Response = response;
        Error = error;
        HttpResponseMessage = httpResponseMessage;
    }

    public T? Response { get; set; }
    public bool Error { get; set; }
    public HttpResponseMessage HttpResponseMessage { get; set; }

    public async Task<string?> GetErrorMessageAsync()
    {
        if (!Error)
        {
            return null;
        }

        return HttpResponseMessage.StatusCode switch
        {
            HttpStatusCode.BadRequest or HttpStatusCode.Conflict =>
                await HttpResponseMessage.Content.ReadAsStringAsync(),
            HttpStatusCode.Unauthorized => "Debes iniciar sesión para realizar esta acción.",
            HttpStatusCode.Forbidden => "No tienes permisos para realizar esta acción.",
            HttpStatusCode.NotFound => "No se encontró el registro solicitado.",
            _ => "No fue posible completar la solicitud. Inténtalo nuevamente."
        };
    }
}
