using System;
using Winit.Shared.Models.Enums;
namespace Winit.Modules.Common.Model.Exceptions;

public class ApiException : Exception
{
    public ExceptionStatus Status { get; set; }
    public int StatusCode { get; set; }
    public ApiException() : base()
    {

    }

    public ApiException(ExceptionStatus status, string? message) : base(message)
    {
        this.Status = status;
    }
    public ApiException(ExceptionStatus status, string? message, int statusCode) : base(message)
    {
        this.Status = status;
        this.StatusCode = statusCode;
    }

    public ApiException(ExceptionStatus status, string? message, Exception? innerException)
        : base(message, innerException)
    {
        this.Status = status;
    }
}
