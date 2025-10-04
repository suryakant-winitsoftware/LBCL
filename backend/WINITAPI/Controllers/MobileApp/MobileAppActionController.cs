using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Azure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Winit.Shared.Models.Enums;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Common;

namespace WINITAPI.Controllers.Mobile
{
    [Route("api/[controller]")]
    [ApiController]
   // [Authorize]
    public class MobileAppActionController : WINITBaseController
    {
        private readonly Winit.Modules.Mobile.BL.Interfaces.IMobileAppActionBL _mobileAppActionBL;
        public MobileAppActionController(IServiceProvider serviceProvider, 
            Winit.Modules.Mobile.BL.Interfaces.IMobileAppActionBL mobileAppActionBL) 
            : base(serviceProvider)
        {
            _mobileAppActionBL = mobileAppActionBL;
        }
        [HttpPost]
        [Route("GetClearDataDetails")]
        public async Task<ActionResult> GetClearDataDetails(PagingRequest pagingRequest)
        {
            try
            {
                if (pagingRequest == null)
                {
                    return BadRequest("Invalid request data");
                }
                if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
                {
                    return BadRequest("Invalid page size or page number");
                }
                PagedResponse<Winit.Modules.Mobile.Model.Interfaces.IMobileAppAction> PagedResponse = null;
                PagedResponse = await _mobileAppActionBL.GetClearDataDetails(pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (PagedResponse == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(PagedResponse);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Fail to retrieve MobileApp Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("GetUserDDL")]
        public async Task<ActionResult> GetUserDDL(string OrgUID)
        {
            try
            {
                if (string.IsNullOrEmpty(OrgUID))
                {
                    return BadRequest("Organization UID is required");
                }

                IEnumerable<Winit.Modules.Mobile.Model.Interfaces.IUser> UserList = null;
                UserList = await _mobileAppActionBL.GetUserDDL(OrgUID);
                if (UserList == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(UserList);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve User with OrgUID: {@OrgUID}", OrgUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);

            }
        }

        [HttpPost]
        [Route("PerformCUD")]
        public async Task<ActionResult> PerformCUD([FromBody]List<Winit.Modules.Mobile.Model.Classes.MobileAppAction>mobileAppActions)
        {
            try
            {
                if (mobileAppActions == null || !mobileAppActions.Any())
                {
                    return BadRequest("Mobile app actions data is required");
                }

                var retVal = await _mobileAppActionBL.PerformCUD(mobileAppActions);
                
                if (retVal > 0)
                {
                    return CreateOkApiResponse(retVal);
                }
                else if (retVal == 0)
                {
                    // Sometimes 0 means success but no rows were affected (update of unchanged data)
                    return CreateOkApiResponse(1);
                }
                else
                {
                    throw new Exception("Insert Failed");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create or update Operations");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }

        }

        [HttpGet]
        [Route("GetMobileAppAction")]
        public async Task<ActionResult> GetMobileAppAction(string empUID)
        {
            try
            {
                if (string.IsNullOrEmpty(empUID))
                {
                    return BadRequest("Employee UID is required");
                }

                Winit.Modules.Mobile.Model.Interfaces.IMobileAppAction mobileAppAction = null;
                mobileAppAction = await _mobileAppActionBL.GetMobileAppAction(empUID);
                if (mobileAppAction == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(mobileAppAction);
            }

            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve MobileAppAction with empUID: {@empUID}", empUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        
        /// <summary>
        /// Test method to verify debugging is working
        /// </summary>
        /// <returns>Simple test response</returns>
        [HttpGet]
        [Route("TestDebug")]
        public ActionResult TestDebug()
        {
            // Set a breakpoint on this line
            var testValue = "Debug test successful";
            
            Log.Information("TestDebug method called successfully");
            
            return CreateOkApiResponse(new { 
                Message = testValue,
                Timestamp = DateTime.Now
            });
        }

        /// <summary>
        /// Initiates database creation process
        /// </summary>
        /// <param name="employeeUID">Employee unique identifier</param>
        /// <param name="jobPositionUID">Job position unique identifier</param>
        /// <param name="roleUID">Role unique identifier</param>
        /// <param name="orgUID">Organization unique identifier</param>
        /// <param name="vehicleUID">Vehicle unique identifier</param>
        /// <param name="empCode">Employee code</param>
        /// <returns>Database creation initiation result</returns>
        [HttpPost]
        [Route("InitiateDBCreation")]
        public async Task<ActionResult> InitiateDBCreation(string employeeUID, string jobPositionUID, string roleUID, string orgUID, string vehicleUID, string empCode)
        {
            try
            {
                // Validate required parameters
                if (string.IsNullOrEmpty(employeeUID))
                {
                    return BadRequest("Employee UID is required");
                }
                if (string.IsNullOrEmpty(jobPositionUID))
                {
                    return BadRequest("Job Position UID is required");
                }
                if (string.IsNullOrEmpty(roleUID))
                {
                    return BadRequest("Role UID is required");
                }
                if (string.IsNullOrEmpty(orgUID))
                {
                    return BadRequest("Organization UID is required");
                }

                Log.Information("Initiating database creation for Employee: {EmployeeUID}, JobPosition: {JobPositionUID}, Role: {RoleUID}, Org: {OrgUID}", 
                    employeeUID, jobPositionUID, roleUID, orgUID);

                var retVal = await _mobileAppActionBL.InitiateDBCreation(employeeUID, jobPositionUID, roleUID, orgUID, vehicleUID, empCode);
                
                if (retVal > 0)
                {
                    Log.Information("Database creation initiated successfully. Request ID: {RequestId}", retVal);
                    return CreateOkApiResponse(new { 
                        RequestId = retVal, 
                        Message = "Database creation initiated successfully",
                        Status = "InProgress"
                    });
                }
                else
                {
                    Log.Warning("Database creation initiation failed for Employee: {EmployeeUID}", employeeUID);
                    return CreateErrorResponse("Failed to initiate database creation", StatusCodes.Status500InternalServerError);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initiate database creation for Employee: {EmployeeUID}, JobPosition: {JobPositionUID}, Role: {RoleUID}", 
                    employeeUID, jobPositionUID, roleUID);
                return CreateErrorResponse("An error occurred while initiating database creation. Please try again later.");
            }
        }

        /// <summary>
        /// Gets the current status of database creation process
        /// </summary>
        /// <param name="employeeUID">Employee unique identifier</param>
        /// <param name="jobPositionUID">Job position unique identifier</param>
        /// <param name="roleUID">Role unique identifier</param>
        /// <returns>Database creation status</returns>
        [HttpGet]
        [Route("GetDBCreationStatus")]
        public async Task<ActionResult> GetDBCreationStatus(string employeeUID, string jobPositionUID, string roleUID)
        {
            try
            {
                // Validate required parameters
                if (string.IsNullOrEmpty(employeeUID))
                {
                    return BadRequest("Employee UID is required");
                }
                if (string.IsNullOrEmpty(jobPositionUID))
                {
                    return BadRequest("Job Position UID is required");
                }
                if (string.IsNullOrEmpty(roleUID))
                {
                    return BadRequest("Role UID is required");
                }

                Log.Information("Checking database creation status for Employee: {EmployeeUID}, JobPosition: {JobPositionUID}, Role: {RoleUID}", 
                    employeeUID, jobPositionUID, roleUID);

                Winit.Modules.Mobile.Model.Interfaces.ISqlitePreparation sqlitePreparation = null;
                sqlitePreparation = await _mobileAppActionBL.GetDBCreationStatus(employeeUID, jobPositionUID, roleUID);
                
                if (sqlitePreparation == null)
                {
                    Log.Warning("No database creation status found for Employee: {EmployeeUID}", employeeUID);
                    return NotFound("No database creation status found for the specified parameters");
                }

                Log.Information("Database creation status retrieved successfully for Employee: {EmployeeUID}. Status: {Status}", 
                    employeeUID, sqlitePreparation.Status);

                return CreateOkApiResponse(sqlitePreparation);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve database creation status for Employee: {EmployeeUID}, JobPosition: {JobPositionUID}, Role: {RoleUID}", 
                    employeeUID, jobPositionUID, roleUID);
                return CreateErrorResponse("An error occurred while retrieving database creation status. Please try again later.");
            }
        }

        /// <summary>
        /// Downloads the SQLite database file
        /// </summary>
        /// <param name="employeeUID">Employee unique identifier</param>
        /// <param name="jobPositionUID">Job position unique identifier</param>
        /// <param name="roleUID">Role unique identifier</param>
        /// <param name="orgUID">Organization unique identifier</param>
        /// <param name="vehicleUID">Vehicle unique identifier</param>
        /// <param name="empCode">Employee code</param>
        /// <returns>Database file download URL or file</returns>
        [HttpPost]
        [Route("DownloadDB")]
        public async Task<ActionResult> DownloadDB([FromBody] dynamic request)
        {
            try
            {
                string employeeUID = request?.employeeUID ?? request?.EmployeeUID;
                string jobPositionUID = request?.jobPositionUID ?? request?.JobPositionUID;
                string roleUID = request?.roleUID ?? request?.RoleUID;
                string orgUID = request?.orgUID ?? request?.OrgUID;
                string vehicleUID = request?.vehicleUID ?? request?.VehicleUID;
                string empCode = request?.empCode ?? request?.EmployeeCode ?? request?.EmpCode;

                // Validate required parameters
                if (string.IsNullOrEmpty(employeeUID))
                {
                    return BadRequest("Employee UID is required");
                }
                if (string.IsNullOrEmpty(jobPositionUID))
                {
                    return BadRequest("Job Position UID is required");
                }
                if (string.IsNullOrEmpty(roleUID))
                {
                    return BadRequest("Role UID is required");
                }
                if (string.IsNullOrEmpty(orgUID))
                {
                    return BadRequest("Organization UID is required");
                }

                Log.Information("DownloadDB called for Employee: {EmployeeUID}, JobPosition: {JobPositionUID}, Role: {RoleUID}, Org: {OrgUID}", 
                    employeeUID, jobPositionUID, roleUID, orgUID);

                // Directly call the sync service to create the database
                var syncRequest = new Winit.Modules.Syncing.Model.Classes.SyncRequest
                {
                    EmployeeUID = employeeUID,
                    JobPositionUID = jobPositionUID,
                    RoleUID = roleUID,
                    OrgUID = orgUID,
                    VehicleUID = vehicleUID,
                    EmployeeCode = empCode,
                    GroupName = "",
                    TableName = ""
                };

                // Get the sync controller through DI
                var syncBL = HttpContext.RequestServices.GetService<Winit.Modules.Syncing.BL.Interfaces.IMobileDataSyncBL>();
                
                if (syncBL == null)
                {
                    Log.Error("Could not resolve IMobileDataSyncBL service");
                    return CreateErrorResponse("Database sync service is not available", StatusCodes.Status503ServiceUnavailable);
                }

                try
                {
                    Log.Information("Creating SQLite database for Employee: {EmployeeUID}", employeeUID);
                    
                    string filePath = await syncBL.DownloadServerDataInSqlite(
                        syncRequest.GroupName, 
                        syncRequest.TableName, 
                        syncRequest.EmployeeUID, 
                        syncRequest.JobPositionUID, 
                        syncRequest.RoleUID,  
                        syncRequest.VehicleUID, 
                        syncRequest.OrgUID, 
                        null);

                    if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
                    {
                        Log.Information("SQLite database created successfully at: {FilePath}", filePath);
                        
                        // Read the file and return it directly
                        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                        var fileName = System.IO.Path.GetFileName(filePath);
                        
                        return File(fileBytes, "application/octet-stream", fileName);
                    }
                    else
                    {
                        Log.Warning("SQLite creation returned no file path or file does not exist");
                        return CreateErrorResponse("Failed to create SQLite database", StatusCodes.Status500InternalServerError);
                    }
                }
                catch (Exception innerEx)
                {
                    Log.Error(innerEx, "Error creating SQLite database for Employee: {EmployeeUID}", employeeUID);
                    return CreateErrorResponse($"Database creation failed: {innerEx.Message}", StatusCodes.Status500InternalServerError);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed in DownloadDB");
                return CreateErrorResponse("An error occurred while preparing database download. Please try again later.");
            }
        }

        /// <summary>
        /// Creates a ZIP file from the SQLite database with proper file handling
        /// </summary>
        /// <param name="sqliteFileName">The SQLite file name to create ZIP from</param>
        /// <returns>The path to the created ZIP file</returns>
        [HttpGet]
        [Route("CreateSqliteZip")]
        public async Task<ActionResult> CreateSqliteZip(string sqliteFileName)
        {
            try
            {
                if (string.IsNullOrEmpty(sqliteFileName))
                {
                    return BadRequest("SQLite file name is required");
                }

                // Get the physical path of the SQLite file
                string dataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Sqlite");
                string sqliteFilePath = Path.Combine(dataPath, sqliteFileName);

                if (!System.IO.File.Exists(sqliteFilePath))
                {
                    return NotFound($"SQLite file not found: {sqliteFileName}");
                }

                // Use SqliteFileHandler to create ZIP with proper file handling
                var fileHandler = new Winit.Modules.Syncing.BL.Classes.SqliteFileHandler();
                string zipFilePath = await fileHandler.CreateZipFromSqlite(sqliteFilePath);

                // Return the relative path for download
                string relativePath = Path.GetRelativePath(Path.Combine(Directory.GetCurrentDirectory(), "Data"), zipFilePath);
                string downloadUrl = $"/data/{relativePath.Replace('\\', '/')}";

                return Ok(new 
                { 
                    success = true, 
                    zipPath = downloadUrl,
                    message = "ZIP file created successfully"
                });
            }
            catch (InvalidOperationException ex)
            {
                Log.Error(ex, "Failed to create ZIP file due to file locking");
                return StatusCode(StatusCodes.Status423Locked, new 
                { 
                    success = false, 
                    error = "SQLite file is currently in use", 
                    message = ex.Message 
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create ZIP file");
                return CreateErrorResponse($"Failed to create ZIP file: {ex.Message}", StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Serves ZIP files from the user-specific directories
        /// </summary>
        /// <param name="fileName">The ZIP file name to serve</param>
        /// <returns>The ZIP file as a download</returns>
        [HttpGet]
        [Route("DownloadZip/{fileName}")]
        public async Task<ActionResult> DownloadZip(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    return BadRequest("File name is required");
                }

                // Search for the file in the user directories
                string dataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Sqlite", "User");
                string zipFilePath = null;

                // Search recursively for the file
                foreach (string file in Directory.GetFiles(dataPath, fileName, SearchOption.AllDirectories))
                {
                    if (Path.GetFileName(file) == fileName)
                    {
                        zipFilePath = file;
                        break;
                    }
                }

                if (zipFilePath == null || !System.IO.File.Exists(zipFilePath))
                {
                    Log.Warning("ZIP file not found: {FileName}", fileName);
                    return NotFound($"ZIP file not found: {fileName}");
                }

                Log.Information("Serving ZIP file: {FilePath}", zipFilePath);

                // Read the file and return it
                var fileBytes = await System.IO.File.ReadAllBytesAsync(zipFilePath);
                var contentType = "application/zip";
                
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to serve ZIP file: {FileName}", fileName);
                return CreateErrorResponse("An error occurred while serving the ZIP file. Please try again later.");
            }
        }

    }
}