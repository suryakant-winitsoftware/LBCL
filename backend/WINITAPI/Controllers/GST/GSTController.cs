using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using WINITServices.Interfaces.CacheHandler;
using Serilog;
using Nest;
namespace WINITAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GSTController : WINITBaseController
    {
        private static readonly HttpClientHandler clientHandler;
        private static readonly HttpClient client;

        static GSTController()
        {
            // Set the security protocol to TLS 1.2 and TLS 1.3
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            clientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
                CheckCertificateRevocationList = false
            };

            client = new HttpClient(clientHandler);
        }

        private readonly string authURL = "https://qa.gsthero.com/auth-server/oauth/token";
        private readonly string username = "gstinapi.sandbox@gsthero.com";
        private readonly string password = "12345678";
        private readonly string clientId = "gstheroclient";
        private readonly string scope = "commonapi";
        private readonly string authProvider = "gstheroclient";
        private readonly string authProviderPassword = "Admin@123";
        private readonly string providerGSTIN = "33GSPTN2102G1ZO";
        private readonly string gstinSearchURL = "https://qa.gsthero.com/commonapi/v1.0/gstin-details";

        public GSTController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        private string _gstInfo;
        [HttpPost("GetGstNumDetail")]
        //public async Task<IActionResult> GetGstNumDetail([FromBody] string requestGST)
        //{
        //    try
        //    {
        //        // Prepare the API request
        //        //  var email = "cadeepaktayal@gmail.com";
        //        //cadeepaktayal%40gmail
        //        var email = "api@wraptax.com";
        //        // var gstin = "33GSPTN1351G1ZF";
        //        //var url = $"http://gstapi.wraptax.in/public/search?email={email}&gstin={requestGST}";
        //        var url = $"https://gstapi.wraptax.com/public/search?email={email}&gstin={requestGST}";
        //        // Set up the request headers
        //        client.DefaultRequestHeaders.Clear();
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //        client.DefaultRequestHeaders.Add("client_id", "GSP4cf84458-bb0f-4bf4-9674-886c97213b50");
        //        client.DefaultRequestHeaders.Add("client_secret", "GSPa591f051-55b0-4ed5-bd24-9477b028fa24");

        //        // Send the GET request
        //        var response = await client.GetAsync(url);
        //        response.EnsureSuccessStatusCode();
        //        //searchResponse.EnsureSuccessStatusCode();

        //        var searchContent = await response.Content.ReadAsStringAsync();
        //        var jsonResponse = JObject.Parse(searchContent);

        //        var statusCd = jsonResponse["data"]["status_cd"]?.ToString() ?? jsonResponse["status_cd"]?.ToString();

        //        if (statusCd == "1")
        //        {
        //            Log.Information("GSTIN search successful for: {GSTIN}", requestGST);
        //            return CreateOkApiResponse(jsonResponse["data"]);
        //        }
        //        else
        //        {
        //            var errorMsg = jsonResponse["data"]["error"]["message"]?.ToString();
        //            var errorCd = jsonResponse["data"]["error"]["error_cd"]?.ToString();
        //            Log.Warning("GSTIN search failed for {GSTIN}. Error Code: {ErrorCode}, Message: {ErrorMessage}", requestGST, errorCd, errorMsg);
        //            return CreateErrorResponse($"Error Code: {errorCd}, Message: {errorMsg}");
        //        }
        //    }
        //    catch (HttpRequestException httpEx)
        //    {
        //        Log.Error(httpEx, "An HTTP error occurred while searching for GSTIN: {GSTIN}. Inner Exception: {InnerException}", requestGST, httpEx.InnerException?.Message);
        //        return StatusCode(500, $"HTTP error: {httpEx.Message}");
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex, "An error occurred while searching for GSTIN: {GSTIN}. Inner Exception: {InnerException}", requestGST, ex.InnerException?.Message);
        //        return StatusCode(500, $"Error: {ex.Message}");
        //    }
        //    finally
        //    {

        //    }
        //}
        public async Task<IActionResult> GetGstNumDetail([FromBody] string requestGST)
        {
            try
            {
                // Prepare the API request
                //  var email = "cadeepaktayal@gmail.com";
                //cadeepaktayal%40gmail
                var email = "api@wraptax.com";
                // var gstin = "33GSPTN1351G1ZF";
                //var url = $"http://gstapi.wraptax.in/public/search?email={email}&gstin={requestGST}";
                var url = $"https://gstapi.wraptax.com/public/search?email={email}&gstin={requestGST}";
                // Set up the request headers
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("client_id", "GSP4cf84458-bb0f-4bf4-9674-886c97213b50");
                client.DefaultRequestHeaders.Add("client_secret", "GSPa591f051-55b0-4ed5-bd24-9477b028fa24");

                // Send the GET request
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                //searchResponse.EnsureSuccessStatusCode();

                var searchContent = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(searchContent);

                var statusCd = jsonResponse["status_cd"]?.ToString() ?? jsonResponse["data"]?["status_cd"]?.ToString();
                if (statusCd == "0")
                {
                    // Handle the case when "status_cd" is missing or invalid
                    Console.WriteLine("Status code is missing or invalid in the response.");
                    var errorMsg = jsonResponse["error"]["message"]?.ToString();
                    var errorCd = jsonResponse["error_cd"]?.ToString();
                    return CreateErrorResponse($"Error Code: {errorCd}, Message: {errorMsg}");
                }
                else if (statusCd == "1")
                {
                    Log.Information("GSTIN search successful for: {GSTIN}", requestGST);
                    return CreateOkApiResponse(jsonResponse["data"]);
                }
                else
                {
                    var errorMsg = jsonResponse["data"]["error"]["message"]?.ToString();
                    var errorCd = jsonResponse["data"]["error"]["error_cd"]?.ToString();
                    Log.Warning("GSTIN search failed for {GSTIN}. Error Code: {ErrorCode}, Message: {ErrorMessage}", requestGST, errorCd, errorMsg);
                    return CreateErrorResponse($"Error Code: {errorCd}, Message: {errorMsg}");
                }
            }
            catch (HttpRequestException httpEx)
            {
                Log.Error(httpEx, "An HTTP error occurred while searching for GSTIN: {GSTIN}. Inner Exception: {InnerException}", requestGST, httpEx.InnerException?.Message);
                return StatusCode(500, $"HTTP error: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while searching for GSTIN: {GSTIN}. Inner Exception: {InnerException}", requestGST, ex.InnerException?.Message);
                return StatusCode(500, $"Error: {ex.Message}");
            }
            finally
            {

            }
        }


        [HttpPost("GstinSearch")]
        public async Task<IActionResult> GstinSearch([FromBody] string requestGST)
        {
            if (string.IsNullOrEmpty(requestGST))
            {
                Log.Error("Received an invalid request: GSTIN is null or empty.");
                return BadRequest("Invalid request");
            }

            try
            {
                Log.Information("Starting GSTIN search for: {GSTIN}", requestGST);

                // Authenticate to get the token
                var authRequest = new HttpRequestMessage(HttpMethod.Post, authURL);

                // Basic Authentication Header
                var base64Auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{authProvider}:{authProviderPassword}"));
                authRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Auth);

                // Headers
                authRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                authRequest.Headers.Add("gstin", providerGSTIN);

                // Form data
                var formData = new MultipartFormDataContent
                {
                    { new StringContent("password"), "grant_type" },
                    { new StringContent(username), "username" },
                    { new StringContent(password), "password" },
                    { new StringContent(clientId), "client_id" },
                    { new StringContent(scope), "scope" }
                };

                authRequest.Content = formData;

                Log.Information("Sending authentication request to {AuthURL}", authURL);
                var authResponse = await client.SendAsync(authRequest);
                authResponse.EnsureSuccessStatusCode();

                var authContent = await authResponse.Content.ReadAsStringAsync();
                var token = JObject.Parse(authContent)["access_token"].ToString();
                Log.Information("Received authentication token successfully.");

                // Use the token to search GSTIN
                var url = new UriBuilder(gstinSearchURL);
                var query = HttpUtility.ParseQueryString(url.Query);
                query["gstin"] = requestGST;
                query["action"] = "TP";
                url.Query = query.ToString();

                var searchRequest = new HttpRequestMessage(HttpMethod.Get, url.ToString());
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                searchRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                searchRequest.Headers.Add("gstin", providerGSTIN);

                Log.Information("Sending GSTIN search request to {GSTINSearchURL}", gstinSearchURL);
                var searchResponse = await client.SendAsync(searchRequest);
                searchResponse.EnsureSuccessStatusCode();

                var searchContent = await searchResponse.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(searchContent);

                var statusCd = jsonResponse["data"]["status_cd"]?.ToString() ?? jsonResponse["status_cd"]?.ToString();

                if (statusCd == "1")
                {
                    Log.Information("GSTIN search successful for: {GSTIN}", requestGST);
                    return CreateOkApiResponse(jsonResponse["data"]);
                }
                else
                {
                    var errorMsg = jsonResponse["data"]["error"]["message"]?.ToString();
                    var errorCd = jsonResponse["data"]["error"]["error_cd"]?.ToString();
                    Log.Warning("GSTIN search failed for {GSTIN}. Error Code: {ErrorCode}, Message: {ErrorMessage}", requestGST, errorCd, errorMsg);
                    return CreateErrorResponse($"Error Code: {errorCd}, Message: {errorMsg}");
                }
            }
            catch (HttpRequestException httpEx)
            {
                Log.Error(httpEx, "An HTTP error occurred while searching for GSTIN: {GSTIN}. Inner Exception: {InnerException}", requestGST, httpEx.InnerException?.Message);
                return StatusCode(500, $"HTTP error: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while searching for GSTIN: {GSTIN}. Inner Exception: {InnerException}", requestGST, ex.InnerException?.Message);
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
}







//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Threading.Tasks;
//using System.Web;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Caching.Memory;
//using Microsoft.Extensions.Hosting;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using WINITAPI.HostedServices;
//using WINITServices.Interfaces.CacheHandler;
//using WINITSharedObjects.Constants;

//namespace WINITAPI.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    [Authorize]
//    public class GSTController : WINITBaseController
//    {
//        private static readonly HttpClient client = new HttpClient();
//        private readonly string authURL = "https://qa.gsthero.com/auth-server/oauth/token";
//        private readonly string username = "gstinapi.sandbox@gsthero.com";
//        private readonly string password = "12345678";
//        private readonly string clientId = "gstheroclient";
//        private readonly string scope = "commonapi";
//        private readonly string authProvider = "gstheroclient";
//        private readonly string authProviderPassword = "Admin@123";
//        private readonly string providerGSTIN = "33GSPTN2102G1ZO";
//        private readonly string gstinSearchURL = "https://qa.gsthero.com/commonapi/v1.0/gstin-details";
//        public GSTController(ICacheService cacheService) 
//            : base(cacheService)
//        {
//        }
//        /// <summary>
//        /// Call this method to start GST Number
//        /// </summary>
//        /// <returns></returns>
//        /// 
//        [HttpPost("GstinSearch")]
//        public async Task<IActionResult> GstinSearch([FromBody] string requestGST)
//        {

//            if (string.IsNullOrEmpty(requestGST))
//            {
//                return BadRequest("Invalid request");
//            }

//            try
//            {
//                // Authenticate to get the token
//                var authRequest = new HttpRequestMessage(HttpMethod.Post, authURL);

//                // Basic Authentication Header
//                var base64Auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{authProvider}:{authProviderPassword}"));
//                authRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Auth);

//                // Headers
//                authRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//                authRequest.Headers.Add("gstin", providerGSTIN);

//                // Form data
//                var formData = new MultipartFormDataContent
//                {
//                    { new StringContent("password"), "grant_type" },
//                    { new StringContent(username), "username" },
//                    { new StringContent(password), "password" },
//                    { new StringContent(clientId), "client_id" },
//                    { new StringContent(scope), "scope" }
//                };

//                authRequest.Content = formData;

//                var authResponse = await client.SendAsync(authRequest);
//                authResponse.EnsureSuccessStatusCode();

//                var authContent = await authResponse.Content.ReadAsStringAsync();
//                var token = JObject.Parse(authContent)["access_token"].ToString();

//                // Use the token to search GSTIN
//                var url = new UriBuilder(gstinSearchURL);
//                var query = HttpUtility.ParseQueryString(url.Query);
//                //query["gstin"] = requestModel.Gstin;
//                query["gstin"] = requestGST;
//                query["action"] = "TP";
//                url.Query = query.ToString();

//                var searchRequest = new HttpRequestMessage(HttpMethod.Get, url.ToString());
//                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

//                searchRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//                searchRequest.Headers.Add("gstin", providerGSTIN);

//                var searchResponse = await client.SendAsync(searchRequest);
//                searchResponse.EnsureSuccessStatusCode();

//                var searchContent = await searchResponse.Content.ReadAsStringAsync();
//                var jsonResponse = JObject.Parse(searchContent);

//                var statusCd = jsonResponse["data"]["status_cd"] != null ? jsonResponse["data"]["status_cd"]?.ToString() : jsonResponse["status_cd"]?.ToString();

//                if (statusCd == "1")
//                {
//                    //  var gstinDetails = ConvertSearchGSTINJsonToModel(jsonResponse["data"]);

//                    //if (gstinDetails != null)
//                    //{
//                    //    SaveToDatabaseUsingStoredProcedure(gstinDetails);
//                    //}
//                    return CreateOkApiResponse(jsonResponse["data"]);
//                }
//                else // statusCd == "0"
//                {
//                    var errorMsg = jsonResponse["data"]["error"]["message"]?.ToString();
//                    var errorCd = jsonResponse["data"]["error"]["error_cd"]?.ToString();
//                    return CreateErrorResponse($"Error Code: {errorCd}, Message: {errorMsg}");
//                }
//            }
//            catch (Exception ex)
//            {
//                return CreateErrorResponse(ex.Message, 500);
//            }
//        }

//        private void SaveToDatabaseUsingStoredProcedure(GstinRequestModel gstinDetails)
//        {
//            // Your implementation here
//        }

//        private GstinRequestModel ConvertSearchGSTINJsonToModel(JToken data)
//        {
//            // Your implementation here
//            return new GstinRequestModel();
//        }
//    }

//    public class GstinRequestModel
//    {
//        public string Gstin { get; set; }
//    }




//}
