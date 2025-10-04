using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Shared.Models.Common;

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public int StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;
    public DateTime? CurrentServerTime { get; set; }

    public ApiResponse(T? data, int statusCode = 200, string? errorMessage = null, DateTime? currentServerTime = null)
    {
        Data = data;
        StatusCode = statusCode;
        ErrorMessage = errorMessage;
        CurrentServerTime = currentServerTime;
    }
}