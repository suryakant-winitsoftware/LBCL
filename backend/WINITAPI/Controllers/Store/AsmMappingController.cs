using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Winit.Modules.Scheme.Model.Classes;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.Model.Classes;
using Winit.Modules.Store.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace WINITAPI.Controllers.Store
{
    [Route("api/[controller]")]
    [ApiController]
    public class AsmMappingController : WINITBaseController
    {
        private readonly IStoreAsmMappingBL _storeAsmMappingBL;
        public AsmMappingController(IServiceProvider serviceProvider, IStoreAsmMappingBL storeAsmMappingBL) : base(serviceProvider)
        {
            _storeAsmMappingBL = storeAsmMappingBL;
        }
        private void ValidateAsmMapping(List<IStoreAsmMapping> newRecords)
        {
            DateTime now = DateTime.Now; // Consider today's date without time for validation
            // Identify duplicate records in the input list
            var duplicateKeys = newRecords
                .GroupBy(e => new { e.CustomerCode, e.SiteCode, e.Division })
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToHashSet(); // Using HashSet for fast lookup
            foreach (var record in newRecords)
            {
                try
                {
                    // Duplicate Key Check
                    if (duplicateKeys.Contains(new { record.CustomerCode, record.SiteCode, record.Division }))
                    {
                        throw new Exception("Duplicate CustomerCode, SiteCode, Division combination.");
                    }

                    // Mandatory field validation
                    if (string.IsNullOrWhiteSpace(record.CustomerCode))
                        throw new Exception("CustomerCode is required.");

                    // Mandatory field validation
                    if (string.IsNullOrWhiteSpace(record.CustomerName))
                        throw new Exception("CustomerName is required.");

                    // Mandatory field validation
                    if (string.IsNullOrWhiteSpace(record.Division))
                        throw new Exception("Division is required.");

                    if (string.IsNullOrWhiteSpace(record.EmpCode))
                        throw new Exception("EmpCode is required.");

                    if (string.IsNullOrWhiteSpace(record.EmpName))
                        throw new Exception("EmpName is required.");


                    record.IsValid = true;
                }
                catch (Exception ex)
                {
                    record.IsValid = false;
                    record.ErrorMessage = ex.Message;
                }
            }
        }
        private async Task<List<IAsmDivisionMapping>> CreateAsmObjects(List<IStoreAsmMapping> storeAsmMappings)
        {
            try
            {
                List<IAsmDivisionMapping> asmDivisionMappings = new List<IAsmDivisionMapping>();
                foreach (var data in storeAsmMappings)
                {
                    AsmDivisionMapping asmDivisionMapping = new AsmDivisionMapping();
                    asmDivisionMapping.UID = Guid.NewGuid().ToString();
                    asmDivisionMapping.CreatedBy = "ADMIN";
                    asmDivisionMapping.CreatedTime = DateTime.Now;
                    asmDivisionMapping.ModifiedBy = "ADMIN";
                    asmDivisionMapping.ModifiedTime = DateTime.Now;
                    asmDivisionMapping.ServerAddTime = DateTime.Now;
                    asmDivisionMapping.ServerModifiedTime = DateTime.Now;
                    asmDivisionMapping.LinkedItemType = string.IsNullOrEmpty(data.SiteCode) ? "Store" : "Address";
                    asmDivisionMapping.LinkedItemUID = string.IsNullOrEmpty(data.SiteCode) ? data.StoreUID : data.SiteUID;
                    asmDivisionMapping.AsmEmpUID = data.EmpUID;
                    asmDivisionMapping.DivisionUID = data.Division;
                    asmDivisionMappings.Add(asmDivisionMapping);
                }
                return asmDivisionMappings;
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost("BulkImportAsmMapping")]
        public async Task<IActionResult> BulkImportAsmMapping([FromBody] List<IStoreAsmMapping> newRecords)
        {
            try
            {
                if (newRecords == null || newRecords.Count == 0)
                    return BadRequest("No records provided.");

                #region Main Validation
                ValidateAsmMapping(newRecords);

                List<IStoreAsmMapping> validRecords = null;
                List<IStoreAsmMapping> invalidRecords = null;

                validRecords = newRecords.Where(e => e.IsValid).ToList() ?? new List<IStoreAsmMapping>();
                invalidRecords = newRecords.Where(e => !e.IsValid).ToList() ?? new List<IStoreAsmMapping>();

                if (validRecords.Count == 0)
                    return Ok(invalidRecords);
                #endregion

                List<IStoreAsmMapping> asmMappingRecords = await _storeAsmMappingBL.GetExistingCustomersList(validRecords, invalidRecords);

                List<IStoreAsmMapping> validAsmRecords = asmMappingRecords.Where(x => string.IsNullOrEmpty(x.ErrorMessage)).ToList();
                List<IStoreAsmMapping> invalidAsmRecords = asmMappingRecords.Where(x => !string.IsNullOrEmpty(x.ErrorMessage)).ToList();

                if(validAsmRecords.Count > 0)
                {
                    List<IAsmDivisionMapping> asmDivisionMappings = await CreateAsmObjects(validAsmRecords);

                    int count = await _storeAsmMappingBL.CUAsmMapping(asmDivisionMappings);
                }
                
                return Ok(invalidAsmRecords);
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        [HttpPost("SelectAllStoreAsmMapping")]
        public async Task<ActionResult> SelectAllStoreAsmMapping([FromBody] PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Store.Model.Interfaces.IAsmDivisionMapping> pagedResponseAddressList = null;
                pagedResponseAddressList = await _storeAsmMappingBL.SelectAllStoreAsmMapping(pagingRequest.SortCriterias,
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

    }
}
