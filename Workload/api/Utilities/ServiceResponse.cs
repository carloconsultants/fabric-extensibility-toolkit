using System.Net;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;

namespace TemplateWorkload.Utilities
{
    /// <summary>
    /// Represents a generic service response with HTTP status and optional error message.
    /// </summary>
    public class ServiceResponse
    {
        public HttpStatusCode Status { get; set; }
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Creates a successful <see cref="ServiceResponse"/>.
        /// </summary>
        /// <returns>A <see cref="ServiceResponse"/> with a status of <see cref="HttpStatusCode.OK"/>.</returns>
        /// <example>
        /// <code>
        /// var response = ServiceResponse.Success();
        /// 
        /// 
        /// </code>
        /// </example>
        public static ServiceResponse Success()
        {
            return new ServiceResponse { Status = HttpStatusCode.OK };
        }

        /// <summary>
        /// Creates an error <see cref="ServiceResponse"/> with the specified status and error message.
        /// </summary>
        /// <param name="status">The HTTP status code representing the error.</param>
        /// <param name="errorMessage">The error message to include in the response.</param>
        /// <returns>A <see cref="ServiceResponse"/> with the specified status and error message.</returns>
        /// <example>
        /// <code>
        /// var response = ServiceResponse.Error(HttpStatusCode.BadRequest, "Invalid request");
        /// 
        /// 
        /// </code>
        /// </example>
        public static ServiceResponse Error(HttpStatusCode status, string errorMessage)
        {
            return new ServiceResponse { Status = status, ErrorMessage = errorMessage };
        }
        public static IActionResult ServiceResponseToIActionResult<T>(ServiceResponse<T> response)
        {

            if (response.Status == HttpStatusCode.OK)
            {
                return new OkObjectResult(response.Data);
            }
            else
            {
                return new ObjectResult(response.ErrorMessage) { StatusCode = (int)response.Status };
            }
        }
    }

    /// <summary>
    /// Represents a service response with HTTP status, optional error message, and generic data payload.
    /// </summary>
    /// <typeparam name="T">The type of data included in the response.</typeparam>
    public class ServiceResponse<T> : ServiceResponse
    {

        public T? Data { get; set; }

        /// <summary>
        /// Creates a successful <see cref="ServiceResponse{T}"/> with the specified data.
        /// </summary>
        /// <param name="data">The data to include in the response.</param>
        /// <returns>A <see cref="ServiceResponse{T}"/> with a status of <see cref="HttpStatusCode.OK"/> and the specified data.</returns>
        /// <example>
        /// <code>
        /// var response = ServiceResponse<string>.Success("Operation successful");
        /// 
        /// 
        /// </code>
        /// </example>
        public static ServiceResponse<T> Success(T data)
        {
            return new ServiceResponse<T> { Status = HttpStatusCode.OK, Data = data };
        }

        /// <summary>
        /// Creates an error <see cref="ServiceResponse{T}"/> with the specified status and error message.
        /// </summary>
        /// <param name="status">The HTTP status code representing the error.</param>
        /// <param name="errorMessage">The error message to include in the response.</param>
        /// <returns>A <see cref="ServiceResponse{T}"/> with the specified status and error message.</returns>
        /// <example>
        /// <code>
        /// var response = ServiceResponse<string>.Error(HttpStatusCode.BadRequest, "Invalid input");
        /// 
        /// 
        /// 
        /// </code>
        /// </example>
        public new static ServiceResponse<T> Error(HttpStatusCode status, string errorMessage)
        {
            return new ServiceResponse<T> { Status = status, ErrorMessage = errorMessage };
        }

        /// <summary>
        /// Creates an error <see cref="ServiceResponse{T}"/> with the specified status, error message, and data.
        /// </summary>
        /// <param name="status">The HTTP status code representing the error.</param>
        /// <param name="errorMessage">The error message to include in the response.</param>
        /// <param name="data">The data to include in the response.</param>
        /// <returns>A <see cref="ServiceResponse{T}"/> with the specified status, error message, and data.</returns>
        /// <example>
        /// <code>
        /// var response = ServiceResponse<string>.ErrorWithData(
        ///     HttpStatusCode.NotFound,
        ///     "Item not found",
        ///     "ItemID: 123"
        /// );
        /// 
        /// 
        /// 
        /// </code>
        /// </example>
        public static ServiceResponse<T> ErrorWithData(HttpStatusCode status, string errorMessage, T data)
        {
            return new ServiceResponse<T> { Status = status, ErrorMessage = errorMessage, Data = data };
        }
    }
}
