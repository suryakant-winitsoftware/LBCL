using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Practice
{
    public class FileName
    {
    }
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResponse<string>> FetchDataAsync(string endpoint, HttpMethod httpMethod, object requestData = null)
        {
            
            var requestMessage = new HttpRequestMessage(httpMethod, endpoint);

            if (httpMethod != HttpMethod.Get && requestData != null)
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
                requestMessage.Content = jsonContent;
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
        private bool IsApiResponseStructure(string responseBody)
        {
            // Implement logic to check if the response matches ApiResponse<T> structure
            // You might want to use regex or other methods to identify the structure.
            // This will depend on your API's response format.
            // For simplicity, let's assume a basic check for now.
            return responseBody.Contains("\"Data\":") && responseBody.Contains("\"StatusCode\":");
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
    public class ApiResponse<T>
    {
        public T Data { get; set; }
        public int StatusCode { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;

        public ApiResponse(T data, int statusCode = 200, string errorMessage = null)
        {
            Data = data;
            StatusCode = statusCode;
            ErrorMessage = errorMessage;
        }
    }
    

   

    
}
