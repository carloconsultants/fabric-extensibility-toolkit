using System.Net;

namespace PowerBITips.Api.Services.Common;

public class ServiceResponse<T>
{
    public HttpStatusCode Status { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsSuccess => Status == HttpStatusCode.OK;

    public static ServiceResponse<T> Success(T data) =>
        new() { Status = HttpStatusCode.OK, Data = data };

    public static ServiceResponse<T> Error(HttpStatusCode status, string message) =>
        new() { Status = status, ErrorMessage = message };

    public static ServiceResponse<T> NotFound(string message = "Resource not found") =>
        new() { Status = HttpStatusCode.NotFound, ErrorMessage = message };

    public static ServiceResponse<T> BadRequest(string message = "Bad request") =>
        new() { Status = HttpStatusCode.BadRequest, ErrorMessage = message };

    public static ServiceResponse<T> Unauthorized(string message = "Unauthorized") =>
        new() { Status = HttpStatusCode.Unauthorized, ErrorMessage = message };

    public static ServiceResponse<T> InternalServerError(string message = "Internal server error") =>
        new() { Status = HttpStatusCode.InternalServerError, ErrorMessage = message };
}