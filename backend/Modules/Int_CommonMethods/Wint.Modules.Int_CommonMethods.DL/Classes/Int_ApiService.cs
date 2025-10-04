using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Int_CommonMethods.DL.Classes
{
    public class Int_ApiService
    {

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public Int_ApiService(HttpClient httpClient,IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }
        public async Task<ApiResponse<string>> FetchDataAsync(string endpoint, HttpMethod httpMethod, object? requestData = null, IDictionary<string, string>? headers = null)
        {
            var requestMessage = new HttpRequestMessage(httpMethod, endpoint);

            if (httpMethod != HttpMethod.Get && requestData != null)
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
                string dat = JsonConvert.SerializeObject(requestData);
                requestMessage.Content = jsonContent;
            }
            string token = CommonFunctions.GetStringValue(_configuration["AppSettings:Apikey"] ?? string.Empty);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    requestMessage.Headers.Add(header.Key, header.Value);
                }
            }
            try
            {
                var response = await _httpClient.SendAsync(requestMessage);
                return await HandleResponse(response);
            }
            catch (HttpRequestException ex)
            {
                // Handle connection errors
                return new ApiResponse<string>(default, 0, "Connection Error: " + ex.Message);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                // Handle timeout errors
                return new ApiResponse<string>(default, 0, "Request Timed Out");
            }
        }
        private async Task<ApiResponse<string>> HandleResponse(HttpResponseMessage response)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return new ApiResponse<string>(responseBody, (int)response.StatusCode, "Success");
            }
            else
            {
                var defaultErrorMessage = GetDefaultErrorMessage(response.StatusCode);
                return new ApiResponse<string>(responseBody, (int)response.StatusCode, defaultErrorMessage);
            }
        }
        private string GetDefaultErrorMessage(HttpStatusCode statusCode)
        {
            switch (statusCode)
            {
                case HttpStatusCode.InternalServerError:
                    return "Internal Server Error";
                case HttpStatusCode.BadRequest:
                    return "Bad Request";
                case HttpStatusCode.Unauthorized:
                    return "Unauthorized";
                case HttpStatusCode.NotFound:
                    return "Resource Not Found";
                case HttpStatusCode.Forbidden:
                    return "Forbidden";
                case HttpStatusCode.MethodNotAllowed:
                    return "Method Not Allowed";
                case HttpStatusCode.TooManyRequests:
                    return "Too Many Requests";
                // Add more cases as needed
                default:
                    return "API Request Failed";
            }
        }
    }
}
