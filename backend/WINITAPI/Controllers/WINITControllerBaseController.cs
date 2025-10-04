using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Winit.Modules.AuditTrail.BL.Interfaces;
using Winit.Modules.AuditTrail.Model.Classes;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Common;
using WINITServices.Interfaces.CacheHandler;

namespace WINITAPI.Controllers
{
    [ApiController]
    public class WINITBaseController : ControllerBase
    {
        //protected readonly IDistributedCache _cache;
        //public WINITBaseController(IDistributedCache cache)
        //{
        //    _cache = cache;
        //}
        protected readonly IServiceProvider _serviceProvider;
        private IAuditTrailHelper _auditTrailHelperBase;
        private ICacheService _cacheServiceBase;
        private ILogger<WINITBaseController> _loggerBase;
        public WINITBaseController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // Lazy-load cache service only when needed
        private IAuditTrailHelper? AuditTrailHelperService
        {
            get => _auditTrailHelperBase ??= _serviceProvider.GetService<IAuditTrailHelper>();
        }
        protected IAuditTrailHelper? _auditTrailHelper => AuditTrailHelperService;
        private ICacheService? CacheService
        {
            get => _cacheServiceBase ??= _serviceProvider.GetService<ICacheService>();
        }
        protected ICacheService? _cacheService => CacheService;
        private ILogger<WINITBaseController>? LoggerService
        {
            get => _loggerBase ??= _serviceProvider.GetService<ILogger<WINITBaseController>>();
        }
        protected ILogger<WINITBaseController>? _logger => LoggerService;


        public static IActionResult SendResponse<T>(T value, string nullMessage = "No Data Found", int nullSuccessCode = 401) where T : class
        {
            if (value == null)
            {
                return new ObjectResult(new { message = nullMessage }) { StatusCode = nullSuccessCode };
            }
            else
            {
                return new ObjectResult(value) { StatusCode = 200 };
            }
        }
        [HttpGet("api/serialized-ok")]
        [Produces("application/json")]
        public OkObjectResult SerializedOk(object values)
        {
            return Ok(JsonConvert.SerializeObject(values));
        }
        /// <summary>
        /// Vishal to use in future
        /// </summary>
        /// <param name="storeList"></param>
        /// <param name="typeIdentifier"></param>
        /// <returns></returns>
        private List<object> CreateConcreteObjects(IEnumerable<IBaseModel> storeList, string typeIdentifier)
        {
            List<object> concreteObjects = new List<object>();

            // Get all assemblies loaded in the current application domain
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Get the types of all concrete classes that implement IBaseModel from all assemblies
            var concreteTypes = assemblies.SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && typeof(IBaseModel).IsAssignableFrom(t));

            // Find the concrete type that matches the type identifier
            Type targetType = concreteTypes.FirstOrDefault(t => t.FullName == typeIdentifier);

            // Create instances of the identified concrete type from storeList
            if (targetType != null)
            {
                foreach (IBaseModel baseModel in storeList)
                {
                    if (baseModel.GetType() == targetType)
                        concreteObjects.Add(baseModel);
                }
            }

            return concreteObjects;
        }
        protected ActionResult CreateOkApiResponse<T>(T data, DateTime? currentServerTime = null)
        {
            var apiResponse = new ApiResponse<T>(data: data, currentServerTime: currentServerTime);
            return Ok(apiResponse);
        }
        protected ActionResult CreateErrorResponse(string errorMessage, int statusCode = StatusCodes.Status500InternalServerError)
        {
            var errorResponse = new ApiResponse<object>(null, statusCode, errorMessage);
            return StatusCode(statusCode, errorResponse);
        }
        protected ActionResult CreateErrorApiResponse(string errorMessage, int statusCode = StatusCodes.Status500InternalServerError)
        {
            return new ObjectResult(new ApiResponse<object>(null, statusCode, errorMessage))
            {
                StatusCode = statusCode
            };
        }
        protected void LogAuditTrailInBackground(AuditTrailEntry auditTrailEntry)
        {
            Task.Run(async () =>
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var auditTrailHelper = scope.ServiceProvider.GetRequiredService<IAuditTrailHelper>();
                    await auditTrailHelper.PublishAuditTrailEntry(auditTrailEntry);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error logging audit trail with data {System.Text.Json.JsonSerializer.Serialize(auditTrailEntry)}");
                }
            });
        }
    }
}