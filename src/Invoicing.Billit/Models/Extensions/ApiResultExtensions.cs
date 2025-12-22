using Regira.Invoicing.Billit.Models.Abstractions;

namespace Regira.Invoicing.Billit.Models.Extensions;

public static class ApiExceptionExtensions
{
    public static async Task<T> ToApiResult<T>(this HttpResponseMessage response, Exception? ex = null)
        where T : IApiResult, new()
    {
        return new T
        {
            IsSuccess = response.IsSuccessStatusCode,
            Error = ex,
            ErrorResponse = ex == null ? null : await response.Content.ReadAsStringAsync(),
            StatusCode = (int)response.StatusCode
        };
    }
}