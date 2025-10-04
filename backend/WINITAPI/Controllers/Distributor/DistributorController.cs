using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Winit.Modules.Distributor.BL.Interfaces;
using Winit.Modules.Distributor.Model.Classes;
using Winit.Shared.Models.Common;
using Winit.Modules.Emp.BL.Interfaces;
using System.Collections.Generic;
using Winit.Modules.Address.BL.Interfaces;
using Winit.Modules.Contact.BL.Interfaces;
using Winit.Modules.Org.BL.Interfaces;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.StoreDocument.BL.Interfaces;
using Winit.Shared.Models.Enums;
using System.Linq;
using Winit.Modules.Contact.Model.Classes;
using Winit.Modules.Currency.BL.Interfaces;
using Winit.Modules.Currency.Model.Interfaces;
using Serilog;
using Winit.Modules.User.Model.Classes;
using WINITAPI.Common;
using Winit.Modules.JobPosition.Model.Interfaces;
using Winit.Modules.Distributor.Model.Interfaces;
using Winit.Modules.JobPosition.BL.Interfaces;

namespace WINITAPI.Controllers.Distributor
{
    [ApiController]
    [Route("api/[Controller]")]
    [Authorize]
    public class DistributorController : WINITBaseController
    {
        private readonly IDistributorBL _distributorBL;
        private readonly IEmpBL _empBL;
        private readonly IStoreBL _storeBL;
        private readonly IStoreAdditionalInfoBL _additionalInfoBL;
        private readonly IOrgBL _orgBL;
        private readonly IStoreCreditBL _creditBL;
        private readonly IContactBL _contactBL;
        private readonly IStoreDocumentBL _storeDocumentBL;
        private readonly IAddressBL _addressBL;
        private readonly ICurrencyBL _currencyBL;
        private readonly RSAHelperMethods _rSAHelperMethods;
        private readonly IJobPositionBL _jobPositionBL;
        public DistributorController(IServiceProvider serviceProvider, 
            IDistributorBL distributorBL, IEmpBL empBL, 
            IStoreBL storeBL, IStoreAdditionalInfoBL additionalInfoBL,
            IOrgBL orgBL, IStoreCreditBL creditBL, 
            IContactBL contactBL, IStoreDocumentBL storeDocumentBL, 
            IAddressBL addressBL,
            ICurrencyBL currencyBL, IJobPositionBL jobPositionBL,
            RSAHelperMethods rSAHelperMethods) : base(serviceProvider)
        {
            _distributorBL = distributorBL;
            _empBL = empBL;
            _storeBL = storeBL;
            _additionalInfoBL = additionalInfoBL;
            _orgBL = orgBL;
            _creditBL = creditBL;
            _contactBL = contactBL;
            _storeDocumentBL = storeDocumentBL;
            _addressBL = addressBL;
            _currencyBL = currencyBL;
            _rSAHelperMethods = rSAHelperMethods;
            _jobPositionBL = jobPositionBL;
        }
        [HttpPost]
        [Route("CreateDistributor")]
        public async Task<ActionResult> CreateDistributor([FromBody] DistributorMasterView distributorMasterView)
        {
            try
            {
                var retVal = await _distributorBL.CreateDistributor(distributorMasterView);
                return (retVal > 0) ? Created("Created", retVal) : throw new Exception("Insert Failed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Distributor details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        

        [HttpPost]
        [Route("SelectAllDistributors")]
        public async Task<ActionResult> SelectAllDistributors([FromBody] PagingRequest pagingRequest)
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
                PagedResponse<Winit.Modules.Distributor.Model.Interfaces.IDistributor> pagedResponse = null;
                pagedResponse = await _distributorBL.SelectAllDistributors(pagingRequest.SortCriterias,
                   pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                   pagingRequest.IsCountRequired);
                if (pagedResponse == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(pagedResponse);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrive Distributor details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetDistributorDetailsByUID")]
        public async Task<ActionResult> GetDistributorDetailsByUID(string UID)
        {
            try
            {
                var response = await _distributorBL.GetDistributorDetailsByUID(UID);
                if (response == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve Distributor with UID: {@UID}", UID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPost]
        [Route("CUDDistributorAdmin")]
        public async Task<ActionResult> CUDDistributorAdmin([FromBody] IDistributorAdminDTO empDTO)
        {
            try
            {
                int count = 0;
                string encryptPassword = string.Empty;

                if (!string.IsNullOrEmpty(empDTO.Emp.EncryptedPassword) && (empDTO.ActionType == DistributorAdminActionType.Add || empDTO.ActionType == DistributorAdminActionType.UpdatePW))
                {
                    encryptPassword = _rSAHelperMethods.EncryptText(empDTO.Emp.EncryptedPassword);
                }
                if (empDTO.ActionType == DistributorAdminActionType.Add)
                {
                    count += await _empBL.CreateEmp(empDTO.Emp, encryptPassword);
                    if (count > 0)
                    {
                        count += await _jobPositionBL.CreateJobPosition(empDTO.JobPosition);
                    }
                }
                else if (empDTO.ActionType == DistributorAdminActionType.UpdateUserName || empDTO.ActionType == DistributorAdminActionType.UpdatePW)
                {
                    count += await _empBL.UpdateEmp(empDTO.Emp, encryptPassword);
                }

                return CreateOkApiResponse(count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create Distributor details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpGet]
        [Route("SelectAllDistributorAdminDetailsByOrgUID")]
        public async Task<ActionResult> SelectAllDistributorAdminDetailsByOrgUID(string OrgUID)
        {
            try
            {
                var response = await _empBL.GetAllEmpDetailsByOrgUID(OrgUID);
                if (response == null)
                {
                    return NotFound();
                }
                return CreateOkApiResponse(response);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrive Distributor details with OrgUID", OrgUID);
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}
