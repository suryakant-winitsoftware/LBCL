using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using System;
using System.Text;
using Winit.Modules.Base.BL.Helper.Interfaces;
using Winit.Modules.PriceLadder.BL.Interfaces;
using Winit.Modules.SKU.BL.Interfaces;
using Winit.Modules.SKU.Model.Interfaces;
using Winit.Modules.SKUClass.BL.Interfaces;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.DropDowns.BL.Interfaces;
using Winit.Modules.Promotion.BL.Interfaces;
using Winit.Modules.Location.BL.Interfaces;
using Winit.Modules.Setting.BL.Interfaces;
using Winit.Modules.Route.BL.Interfaces;
using WINITAPI;
using WINITServices.Interfaces.CacheHandler;
using Winit.Shared.Models.Common;
using Winit.Modules.Store.Model.Classes;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Winit.Shared.CommonUtilities.Common;
using Winit.Modules.Auth.Model.Classes;
using Winit.Modules.Auth.Model.Interfaces;
using System.Text.Json;
using System.Net.Http.Headers;

namespace IntegrationTest.WINITAPIController;

/// <summary>
/// Base class for integration tests providing common setup and utilities for testing WINIT API controllers.
/// </summary>
public abstract class BaseIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly WebApplicationFactory<Program> _factory;
    protected readonly HttpClient _client;
    protected readonly Mock<ISKUBL> _mockSKUBL;
    protected readonly Mock<ISortHelper> _mockSortHelper;
    protected readonly Mock<ISKUPriceLadderingBL> _mockSKUPriceLadderingBL;
    protected readonly Mock<ISKUClassGroupItemsBL> _mockSKUClassGroupItemsBL;
    protected readonly Mock<ICacheService> _mockCacheService;
    protected readonly Mock<IStoreBL> _mockStoreBL;
    protected readonly Mock<ISKUPriceListBL> _mockSKUPriceListBL;
    protected readonly Mock<ISKUGroupTypeBL> _mockSKUGroupTypeBL;
    protected readonly Mock<IDropDownsBL> _mockDropDownsBL;
    protected readonly Mock<IPromotionBL> _mockPromotionBL;
    protected readonly Mock<ILocationBL> _mockLocationBL;
    protected readonly Mock<ISettingBL> _mockSettingBL;
    protected readonly Mock<IRouteBL> _mockRouteBL;
    protected readonly Mock<IOptions<ApiSettings>> _mockApiSettings;
    protected readonly SHACommonFunctions _shaCommonFunctions;

    // Test credentials - can be set via environment variables or configuration
    protected readonly string _testUsername;
    protected readonly string _testPassword;

    protected BaseIntegrationTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        
        // Get credentials from environment variables or use defaults
        _testUsername = Environment.GetEnvironmentVariable("WINIT_TEST_USERNAME") ?? "admin";
        _testPassword = Environment.GetEnvironmentVariable("WINIT_TEST_PASSWORD") ?? "PUT_YOUR_ADMIN_PASSWORD_HERE";
        
        // Validate that credentials are provided
        if (_testPassword == "PUT_YOUR_ADMIN_PASSWORD_HERE")
        {
            throw new InvalidOperationException(
                "Please either:\n" +
                "1. Set WINIT_TEST_PASSWORD environment variable with your actual password, OR\n" +
                "2. Replace 'PUT_YOUR_ADMIN_PASSWORD_HERE' with your actual admin password in BaseIntegrationTest.cs\n" +
                "Username is set to 'admin'.");
        }
        
        // Initialize mocks
        _mockSKUBL = new Mock<ISKUBL>();
        _mockSortHelper = new Mock<ISortHelper>();
        _mockSKUPriceLadderingBL = new Mock<ISKUPriceLadderingBL>();
        _mockSKUClassGroupItemsBL = new Mock<ISKUClassGroupItemsBL>();
        _mockCacheService = new Mock<ICacheService>();
        _mockStoreBL = new Mock<IStoreBL>();
        _mockSKUPriceListBL = new Mock<ISKUPriceListBL>();
        _mockSKUGroupTypeBL = new Mock<ISKUGroupTypeBL>();
        _mockDropDownsBL = new Mock<IDropDownsBL>();
        _mockPromotionBL = new Mock<IPromotionBL>();
        _mockLocationBL = new Mock<ILocationBL>();
        _mockSettingBL = new Mock<ISettingBL>();
        _mockRouteBL = new Mock<IRouteBL>();
        _mockApiSettings = new Mock<IOptions<ApiSettings>>();

        // Setup ApiSettings mock
        _mockApiSettings.Setup(x => x.Value).Returns(new ApiSettings());

        // Setup SKUBL PrepareSKUMaster method to return empty list to avoid null reference issues
        _mockSKUBL.Setup(x => x.PrepareSKUMaster(It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()))
                  .ReturnsAsync(new List<ISKUMaster>());

        // Create client with custom services
        _client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace services with mocks for testing - using Singleton lifetime to match DataPreparationController
                services.AddSingleton(_ => _mockSKUBL.Object);
                services.AddSingleton(_ => _mockSortHelper.Object);
                services.AddSingleton(_ => _mockSKUPriceLadderingBL.Object);
                services.AddSingleton(_ => _mockSKUClassGroupItemsBL.Object);
                services.AddSingleton(_ => _mockCacheService.Object);
                services.AddSingleton(_ => _mockStoreBL.Object);
                services.AddSingleton(_ => _mockSKUPriceListBL.Object);
                services.AddSingleton(_ => _mockSKUGroupTypeBL.Object);
                services.AddSingleton(_ => _mockDropDownsBL.Object);
                services.AddSingleton(_ => _mockPromotionBL.Object);
                services.AddSingleton(_ => _mockLocationBL.Object);
                services.AddSingleton(_ => _mockSettingBL.Object);
                services.AddSingleton(_ => _mockRouteBL.Object);
                services.AddSingleton(_ => _mockApiSettings.Object);

                // Configure logging for tests
                services.AddLogging(builder => builder.AddConsole());
            });
        }).CreateClient();

        _shaCommonFunctions = new SHACommonFunctions();
        
        // Authenticate and set authorization header for all requests
        Task.Run(async () => await AuthenticateAsync()).Wait();
    }

    /// <summary>
    /// Authenticates with the API using real credentials and sets the Authorization header.
    /// </summary>
    /// <returns>Task representing the authentication operation.</returns>
    protected async Task AuthenticateAsync()
    {
        try
        {
            // Generate challenge code
            string challengeCode = _shaCommonFunctions.GenerateChallengeCode();
            
            // Encrypt password with challenge
            string encryptedPassword = _shaCommonFunctions.EncryptPasswordWithChallenge(_testPassword, challengeCode);
            
            // Create login request
            var userLogin = new UserLogin
            {
                UserID = _testUsername,
                Password = encryptedPassword,
                ChallengeCode = challengeCode,
                DeviceId = "integration-test-device"
            };

            // Serialize the login request
            var json = JsonSerializer.Serialize(userLogin);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Call the authentication endpoint
            var response = await _client.PostAsync("/api/Auth/GetToken", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (loginResponse?.Token != null)
                {
                    // Set the Authorization header for all subsequent requests
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
                }
                else
                {
                    throw new InvalidOperationException("Authentication failed: No token received");
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Authentication failed with status {response.StatusCode}: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Authentication setup failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Re-authenticates if the current token has expired.
    /// </summary>
    /// <returns>Task representing the re-authentication operation.</returns>
    protected async Task ReAuthenticateIfNeededAsync()
    {
        // Check if we have an authorization header
        if (_client.DefaultRequestHeaders.Authorization == null)
        {
            await AuthenticateAsync();
        }
    }

    /// <summary>
    /// Creates HTTP content from an object by serializing it to JSON.
    /// </summary>
    /// <param name="obj">The object to serialize</param>
    /// <returns>StringContent with JSON data</returns>
    protected static StringContent CreateJsonContent(object obj)
    {
        var json = JsonConvert.SerializeObject(obj);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    /// <summary>
    /// Deserializes HTTP response content to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to</typeparam>
    /// <param name="response">The HTTP response</param>
    /// <returns>Deserialized object</returns>
    protected static async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(content);
    }

    /// <summary>
    /// Creates a sample ISKUV1 object for testing purposes.
    /// </summary>
    /// <param name="uid">Optional UID for the SKU</param>
    /// <returns>Sample ISKUV1 object</returns>
    protected static ISKUV1 CreateSampleSKUV1(string uid = null)
    {
        return new Mock<ISKUV1>().Object;
    }

    /// <summary>
    /// Creates a sample SKU object for testing purposes.
    /// </summary>
    /// <param name="uid">Optional UID for the SKU</param>
    /// <returns>Sample SKU object</returns>
    protected static ISKU CreateSampleSKU(string uid = null)
    {
        return new Mock<ISKU>().Object;
    }

    /// <summary>
    /// Disposes of resources used by the test.
    /// </summary>
    public virtual void Dispose()
    {
        _client?.Dispose();
    }
} 