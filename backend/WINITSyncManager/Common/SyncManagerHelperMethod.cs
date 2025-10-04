using Azure;
using SyncManagerBL.Classes;
using SyncManagerBL.Interfaces;
using SyncManagerModel.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;

namespace WINITSyncManager.Common
{
    public class SyncManagerHelperMethod
    {
        public readonly IEntityScriptBL _entityScriptBL;

        public SyncManagerHelperMethod(IEntityScriptBL entityScriptBL)
        {
            _entityScriptBL = entityScriptBL;

        }
        public static async Task<HttpResponseMessage> PostDataAsync(string url, string json)
        {
            HttpResponseMessage response;
            HttpClient client = new HttpClient();
            var content = new StringContent(json, Encoding.UTF8, "application/json-patch+json");

            // Replace with your actual JWS token
            // string jwsToken = "your_jws_token_here";
            try
            {
                // Add the JWS token to the Authorization headerd
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwsToken);

                response = await client.PostAsync(url, content);

                // Ensure the response is successful
                response.EnsureSuccessStatusCode();

                // Get the response content
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response: " + responseBody);
                return response;
            }
            catch (HttpRequestException e)
            {
                // Log the exception details
                Console.WriteLine($"Request error: {e.Message}");
                throw;
            }
            catch (TaskCanceledException e)
            {
                // Handle request timeout or cancellation
                Console.WriteLine($"Request timed out: {e.Message}");
                throw;
            }
            catch (Exception e)
            {
                // Catch-all for any other exceptions
                Console.WriteLine($"An error occurred: {e.Message}");
                throw;
            }
        }
        public async Task<string> GetSelectScriptByEntity(string Entity)
        {
            try
            {
                SyncManagerModel.Interfaces.IEntityScript entityScript = await _entityScriptBL.GetEntityScriptDetailsByEntity(Entity);
                if (entityScript == null) throw new Exception($"No select query available for Entity: {Entity}");
                 return entityScript != null ? entityScript.SelectQuery + @$" FETCH FIRST {(entityScript.MaxCount==0 ?10:entityScript.MaxCount)} ROWS ONLY " : string.Empty;
            }
            catch
            {
                throw;
            }
        }

        // Method to get and deserialize dynamic data from GET API
        public static async Task<List<T>?> GetDynamicDataAsync<T>(string apiUrl)
        {
            try
            {
                HttpClient _httpClient = new HttpClient();
                // Send GET request
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                // Ensure success status code
                response.EnsureSuccessStatusCode();

                // Read the response content as a string
                string responseData = await response.Content.ReadAsStringAsync();

                // Parse the response dynamically using JsonSerializer
                ApiResponse<List<T>>? apiResponse = JsonSerializer.Deserialize<ApiResponse<List<T>>>(responseData, new JsonSerializerOptions { WriteIndented = true });
                return apiResponse?.Data;
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP request errors
                Console.WriteLine($"Request error: {ex.Message}");
                return default;
            }
        }
    }

}
