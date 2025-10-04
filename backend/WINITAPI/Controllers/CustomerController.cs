using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using WINITRepository.Interfaces.Customers;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
using static WINITRepository.Classes.Customers.PostgreSQLCustomerRepository;

namespace WINITAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class CustomerController : WINITBaseController
    {
        private readonly WINITServices.Interfaces.ICustomerService _customerService;

        public CustomerController(IServiceProvider serviceProvider, WINITServices.Interfaces.ICustomerService customerService) : base(serviceProvider)
        {
            _customerService = customerService;
        }

        [HttpGet]
        [Route("SelectAllCustomersRedis")]
        public async Task<ActionResult<IEnumerable<Customer>>> SelectAllCustomersRedis()
        {
            try
            {
                var cacheKey = $"Customer{0}"; ;
                IEnumerable<Customer> customers = null;

                // Try to get the data from the cache
                customers = _cacheService.Get<IEnumerable<Customer>>(cacheKey);
                if (customers != null)
                {
                    return Ok(customers);
                }

                // If the data is not in the cache, fetch it from the service
                customers = await _customerService.SelectAllCustomers();
                if (customers == null)
                {
                    return NotFound();
                }
                else
                {
                    // Cache the data
                    foreach(Customer customer in customers)
                    {
                        _cacheService.Set(cacheKey, customers, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
                    }
                    _cacheService.Set(cacheKey, customers, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
                }
                Log.Information("Successfully retrieved all customers details {@customers}", customers);
                return Ok(customers);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve customers data");
                return StatusCode(StatusCodes.Status500InternalServerError, "Fail to retrieve customers data");
            }
        }

        [HttpGet]
        [Route("SelectAllCustomers")]
        public async Task<ActionResult<IEnumerable<Customer>>> SelectAllCustomers()
        {
            try
            {
                /* // Retrieve the authenticated user's identity and claims
                 ClaimsIdentity identity = HttpContext.User.Identity as ClaimsIdentity;
                 if (identity != null)
                 {
                     // Retrieve the payload information from the JWT token's claims
                     string username = identity.FindFirst(ClaimTypes.Name)?.Value;
                     string permissions = string.Join(",", identity.FindAll("permissions").Select(c => c.Value));

                     // Validate the payload information
                     if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(permissions))
                     {
                         return Unauthorized();
                     }

                     // Check if the user has read permission
                     if (!permissions.Contains("read"))
                     {
                         return StatusCode(StatusCodes.Status403Forbidden, "You do not have permission to read customer data.");
                     }
                 }*/

                var cacheKey = WINITSharedObjects.Constants.CacheConstants.ALL_CUSTOMERS;
                object customers = null;

                // Try to get the data from the cache
                customers = _cacheService.Get<object>(cacheKey);
                if (customers != null)
                {
                    return Ok(customers);
                }

                // If the data is not in the cache, fetch it from the service
                customers = await _customerService.SelectAllCustomers();
                if (customers == null)
                {
                    return NotFound();
                }
                else
                {
                    // Cache the data
                    _cacheService.Set(cacheKey, customers, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
                }
                Log.Information("Successfully retrieved all customers details {@customers}", customers);
                return Ok(customers);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve customers data");
                return StatusCode(StatusCodes.Status500InternalServerError, "Fail to retrieve customers data");
            }
        }

        [HttpGet]
        [Route("SelectCustomerByUID")]
        public async Task<ActionResult> SelectCustomerByUID([FromQuery] int Id)
        {
            try
            {
                WINITSharedObjects.Models.Customer customer = await _customerService.SelectCustomerByUID(Id);
                Log.Information("Successfully retrieved customer details: {@CustomerDetails}", customer);

                if (customer != null)
                {
                    return Ok(customer);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve customer with ID: {@ID}",Id);
                throw;
            }
        }



      

        [HttpPost]
        [Route("CreateCustomer")]
        public async Task<ActionResult<Customer>> CreateCustomer([FromBody] Customer customer)
        {
            try
            {
                customer.CreatedDate = DateTime.UtcNow;
                var createdCustomer = await _customerService.CreateCustomer(customer);
                Log.Information("Successfully created customer details: {@CustomerDetails}", createdCustomer);
                return Created("", createdCustomer); // Returns 201 Created with the created customer object

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create customer details");
                return StatusCode(500, new { success = false, message = "Error creating customer", error = ex.Message });
            }
            //await Task.Delay(1000);
            //return null;
        }



        [HttpPut]
        [Route("UpdateCustomer")]
        public async Task<ActionResult<Customer>> UpdateCustomer([FromQuery] int Id, [FromBody] Customer Customer)
        {
            try
            {
                var existingCustomer = await _customerService.SelectCustomerByUID(Id);
                if (existingCustomer != null)
                {
                    Customer.LastModifiedDate = DateTime.UtcNow;
                    var updatedcustomer = await _customerService.UpdateCustomer(Id, Customer);
                    Log.Information("Successfully updated customer details:{@updatecutomers}", updatedcustomer);

                    // Return the updated customer object
                    return Ok(updatedcustomer);
                }
                else
                {
                    return NotFound();
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating customer");
                return StatusCode(500, new { success = false, message = "Error updating customer", error = ex.Message });
            }
            //await Task.Delay(1000);
            //return null;
        }

        [HttpDelete]
        [Route("DeleteCustomer")]
        public async Task<ActionResult> DeleteCustomer([FromQuery] int Id)
        {
            try
            {
                var existingCustomer = await _customerService.SelectCustomerByUID(Id);
                if (existingCustomer == null)
                {
                    return NotFound();
                }

                var result = await _customerService.DeleteCustomer(Id);
                Log.Information("Deleted successfully customer ");
                return Ok("Deleted successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Deleting Failuer");
                return StatusCode(500, new { success = false, message = "Deleting Failuer", error = ex.Message });
            }
            //await Task.Delay(1000);
            //return null;
        }

        [HttpGet]
        [Route("GetCustomersFiltered")]
        public async Task<ActionResult> GetCustomersFiltered([FromQuery] string Name, [FromQuery] string Email)
        {
            try
            {
                var customers = await _customerService.GetCustomersFiltered(Name, Email);
                if (customers.Any())
                {
                    Log.Information("Successfully filtered customer details: {@CustomerDetails}", customers);
                    return Ok(customers);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to filter customer data");
                throw;
            }
        }

        [HttpGet]
        [Route("GetCustomersPaged")]
        public async Task<ActionResult> GetCustomersPaged([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            try
            {
                var customers = await _customerService.GetCustomersPaged(pageNumber, pageSize);
                Log.Information("Pagination Successfully : {@CustomerDetails}", customers);
                return Ok(customers);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Pagaination Failed");
                throw;
            }
        }

        [HttpGet]
        [Route("GetCustomersSorted")]
        public async Task<ActionResult> GetCustomersSorted([FromQuery] List<SortCriteria> sortCriterias)
        {
            try
            {
                var customers = await _customerService.GetCustomersSorted(sortCriterias);
                Log.Information("Sorted Successfully: {@CustomerDetails}", customers);
                return Ok(customers);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Sort Failed");
                throw;
            }
        }



        //[HttpGet]
        //[Route("SelectAllCustomersV1")]
        //public async Task<IActionResult> SelectAllCustomersV1()
        //{
        //    var cacheKey = WINIT.Constants.CacheConstants.ALL_CUSTOMERS;

        //    // Try to get products from cache
        //    var cachedProducts = await _cache.GetAsync(cacheKey);
        //    if (cachedProducts != null)
        //    {
        //        var products = JsonSerializer.Deserialize<List<Product>>(cachedProducts);
        //        return Ok(products);
        //    }

        //    // If products aren't in cache, retrieve from database and set cache
        //    var productsFromDatabase = _context.Products.ToList();

        //    // Serialize the products to a byte array for caching
        //    var serializedProducts = JsonSerializer.SerializeToUtf8Bytes(productsFromDatabase);

        //    // Set the products in cache with a 1-hour expiration time
        //    var options = new DistributedCacheEntryOptions()
        //        .SetSlidingExpiration(TimeSpan.FromHours(1));
        //    await _cache.SetAsync(cacheKey, serializedProducts, options);

        //    return Ok(productsFromDatabase);
        //}

        //public CustomerController(WINITServices.Interfaces.ICustomerService customerService)
        //{
        //    _customerService = customerService;
        //}

        //[HttpGet]
        //[Route("SelectAllCustomers")]
        //public async Task<IActionResult> SelectAllCustomers()
        //{
        //    try
        //    {
        //        /*
        //         * Vishal commented temporarily
        //        // Retrieve the authenticated user's identity and claims
        //        ClaimsIdentity identity = HttpContext.User.Identity as ClaimsIdentity;
        //        if (identity != null)
        //        {
        //            // Retrieve the payload information from the JWT token's claims
        //            string username = identity.FindFirst(ClaimTypes.Name)?.Value;
        //            string permissions = string.Join(",", identity.FindAll("permissions").Select(c => c.Value));

        //            // Validate the payload information
        //            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(permissions))
        //            {
        //                return Unauthorized();
        //            }

        //            // Check if the user has read permission
        //            if (!permissions.Contains("read"))
        //            {
        //                return StatusCode(StatusCodes.Status403Forbidden, "You do not have permission to read customer data.");
        //            }
        //        }
        //        */
        //        //// Retrieve the customer data from the service
        //        //var customers = await _customerService.SelectAllCustomers();
        //        //Log.Information("Successfully retrieved all customers details {@customers}", customers);
        //        //return SendResponse(customers);

        //        var cacheKey = WINITSharedObjects.Constants.CacheConstants.ALL_CUSTOMERS;
        //        object customers = null;

        //        // Try to get the data from the cache
        //        //bool IsAvailable = _memoryCache.TryGetValue(cacheKey, out customers);
        //        customers = _cacheService.Get<object>(cacheKey);
        //        // Try to get the product from the cache
        //        if (customers != null)
        //        {
        //            return SendResponse(customers);
        //        }

        //        // If the product is not in the cache, fetch it from the database
        //        customers = await _customerService.SelectAllCustomers();
        //        if (customers == null)
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            // Cache the data
        //            _cacheService.Set(cacheKey, customers, WINITServices.Classes.CacheHandler.ExpirationType.Absolute, 120);
        //        }
        //        Log.Information("Successfully retrieved all customers details {@customers}", customers);
        //        return SendResponse(customers);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex, "Fail to retrieve for customers data");
        //        return StatusCode(StatusCodes.Status500InternalServerError, "Fail to retrieve for customers data");
        //    }
        //}
    }
}