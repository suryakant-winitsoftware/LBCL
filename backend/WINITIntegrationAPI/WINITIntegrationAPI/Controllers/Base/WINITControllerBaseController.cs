using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Winit.Modules.Base.Model;
using Winit.Shared.Models.Common;

namespace WINITAPI.Controllers
{
    [ApiController]
    public class WINITBaseController : ControllerBase
    {
        
        public WINITBaseController()
        {
        }
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
            var apiResponse = new ApiResponse<T>(data:data, currentServerTime: currentServerTime);
            return Ok(apiResponse);
        }
        protected ActionResult CreateErrorResponse(string errorMessage, int statusCode = StatusCodes.Status500InternalServerError)
        {
            var errorResponse = new ApiResponse<object>(null, statusCode, errorMessage);
            return StatusCode(statusCode, errorResponse);
        }
    }
}