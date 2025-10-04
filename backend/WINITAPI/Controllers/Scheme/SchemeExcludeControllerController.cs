using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MongoDB.Driver.Core.Configuration;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Winit.Modules.Scheme.BL.Interfaces;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Shared.CommonUtilities.Common;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;

namespace WINITAPI.Controllers.Scheme
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchemeExcludeControllerController : WINITBaseController
    {
        private ISchemeExcludeMappingBL _schemeExcludeMappingBL;

        public SchemeExcludeControllerController(IServiceProvider serviceProvider,
            ISchemeExcludeMappingBL schemeExcludeMappingBL)
            : base(serviceProvider)
        {
            _schemeExcludeMappingBL = schemeExcludeMappingBL;
        }

        private async Task ValidateSchemeExcludeMapping(List<SchemeExcludeMapping> newRecords)
        {
            DateTime now = DateTime.Now; // Consider today's date without time for validation
            // Identify duplicate records in the input list
            var duplicateKeys = newRecords
                .GroupBy(e => new { e.SchemeType, e.SchemeUID, e.StoreUID })
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToHashSet(); // Using HashSet for fast lookup
            foreach (var record in newRecords)
            {
                try
                {
                    // Duplicate Key Check
                    if (duplicateKeys.Contains(new { record.SchemeType, record.SchemeUID, record.StoreUID }))
                    {
                        throw new Exception("Duplicate SchemeType, SchemeUID, StoreUID combination.");
                    }

                    // Mandatory field validation
                    if (string.IsNullOrWhiteSpace(record.StoreUID))
                        throw new Exception("SchemeType is required.");

                    // Mandatory field validation
                    if (string.IsNullOrWhiteSpace(record.StoreUID))
                        throw new Exception("SchemeUID is required.");

                    // Mandatory field validation
                    if (string.IsNullOrWhiteSpace(record.StoreUID))
                        throw new Exception("StoreUID is required.");

                    // Mandatory field validation
                    if (record.StartDate == DateTime.MinValue)
                        throw new Exception("StartDate is required.");

                    // Mandatory field validation
                    if (record.EndDate == DateTime.MinValue)
                        throw new Exception("EndDate is required.");

                    // Date validation
                    if (record.StartDate.Date < now.Date)
                        throw new Exception("Start Date cannot be in the past.");

                    if (record.EndDate.Date < now.Date)
                        throw new Exception("End Date cannot be in the past.");

                    if (record.EndDate.Date < record.StartDate.Date)
                        throw new Exception("End Date cannot be earlier than Start Date.");

                    if (string.IsNullOrEmpty(
                            await _schemeExcludeMappingBL.CheckIfUIDExistsInDB(DbTableName.Store, record.StoreUID)))
                        throw new Exception("StoreUID is Invalid");

                    record.IsValid = true;
                }
                catch (Exception ex)
                {
                    record.IsValid = false;
                    record.ErrorMessage = ex.Message;
                }
            }
        }

        [HttpPost("BulkImport")]
        public async Task<IActionResult> BulkImport([FromBody] List<SchemeExcludeMapping> newRecords)
        {
            try
            {
                if (newRecords == null || newRecords.Count == 0)
                    return BadRequest("No records provided.");

                DateTime now = DateTime.Now; // Consider today's date without time for validation

                List<SchemeExcludeMapping> validRecords = null;
                List<SchemeExcludeMapping> invalidRecords = null;

                await ValidateSchemeExcludeMapping(newRecords);

                validRecords = newRecords
                                   .Where(e => e.IsValid)
                                   .ToList()
                               ?? new List<SchemeExcludeMapping>();

                invalidRecords = newRecords
                                     .Where(e => !e.IsValid)
                                     .ToList()
                                 ?? new List<SchemeExcludeMapping>();

                // Return if no valid records exist after validation
                if (validRecords.Count == 0)
                    return Ok(invalidRecords);

                await _schemeExcludeMappingBL.BulkImport(validRecords);

                return Ok(invalidRecords);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost("SelectAllSchemeExcludeMapping")]
        public async Task<ActionResult> SelectAllSchemeExcludeMapping([FromBody] PagingRequest pagingRequest)
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

                PagedResponse<Winit.Modules.Scheme.Model.Interfaces.ISchemeExcludeMapping> pagedResponseAddressList =
                    null;
                pagedResponseAddressList = await _schemeExcludeMappingBL.SelectAllSchemeExcludeMapping(
                    pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseAddressList == null)
                {
                    return NotFound();
                }

                return CreateOkApiResponse(pagedResponseAddressList);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost("SelectAllSchemeExcludeMappingHistory")]
        public async Task<ActionResult> SelectAllSchemeExcludeMappingHistory([FromBody] PagingRequest pagingRequest)
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

                PagedResponse<Winit.Modules.Scheme.Model.Interfaces.ISchemeExcludeMapping> pagedResponseAddressList =
                    null;
                pagedResponseAddressList = await _schemeExcludeMappingBL.SelectAllSchemeExcludeMappingHistory(
                    pagingRequest.SortCriterias,
                    pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                    pagingRequest.IsCountRequired);
                if (pagedResponseAddressList == null)
                {
                    return NotFound();
                }

                return CreateOkApiResponse(pagedResponseAddressList);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("CheckSchemeExcludeMappingExists")]
        public async Task<IActionResult> CheckSchemeExcludeMappingExists(
            [FromBody] SchemeExcludeMappingRequest schemeExcludeMappingRequest)
        {
            if (schemeExcludeMappingRequest == null || schemeExcludeMappingRequest?.StoreUID is null)
            {
                return BadRequest("Invalid request data");
            }

            try
            {
                return CreateOkApiResponse<int>(
                    await _schemeExcludeMappingBL.CheckSchemeExcludeMappingExists(schemeExcludeMappingRequest.StoreUID,
                        schemeExcludeMappingRequest.CurrentDate));
            }
            catch (Exception ex)
            {
                return CreateErrorResponse(ex.Message);
            }
        }
    }
}