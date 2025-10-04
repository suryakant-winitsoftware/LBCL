using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Serilog;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Shared.Models.Constants;

namespace WINITAPI.Controllers.Store
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StoreAdditionalInfoCMIController : WINITBaseController
    {
        private readonly Winit.Modules.Store.BL.Interfaces.IStoreAdditionalInfoCMIBL _storeAdditionalInfocmiBL;
        public StoreAdditionalInfoCMIController(IServiceProvider serviceProvider, 
            IStoreAdditionalInfoCMIBL storeAdditionalInfocmiBL) : base(serviceProvider)
        {
            _storeAdditionalInfocmiBL= storeAdditionalInfocmiBL;
        }
        [HttpPost]
        [Route("CreateStoreAdditionalInfoCMI")]
        public async Task<ActionResult> CreateStoreAdditionalInfo([FromBody] Winit.Modules.Store.Model.Classes.StoreAdditionalInfoCMI storeAdditionalInfocmi)
        {
            try
            {
                storeAdditionalInfocmi.ServerAddTime = DateTime.Now;
                storeAdditionalInfocmi.ServerModifiedTime = DateTime.Now;
                var retVal = await _storeAdditionalInfocmiBL.CreateStoreAdditionalInfoCMI(storeAdditionalInfocmi);

                if (retVal > 0)
                {
                    return CreateOkApiResponse(retVal);

                }
                else
                {
                    throw new Exception("Insert Failed");
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create StoreAdditionalInfo details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
        [HttpPut]
        [Route("UpdateStoreAdditionalInfoCMI")]
        public async Task<ActionResult> UpdateStoreAdditionalInfoCMI([FromBody] Winit.Modules.Store.Model.Classes.StoreAdditionalInfoCMI storeAdditionalInfoCMI)
        {
            try
            {
                int retVal=0;
                var existingStoreAdditionalInfoList = await _storeAdditionalInfocmiBL.SelectStoreAdditionalInfoCMIByUID(storeAdditionalInfoCMI.UID);
                if (existingStoreAdditionalInfoList != null)
                {
                    storeAdditionalInfoCMI.ModifiedTime = DateTime.Now;
                    storeAdditionalInfoCMI.ServerModifiedTime = DateTime.Now;
                    if(storeAdditionalInfoCMI.SectionName == OnboardingScreenConstant.BusinessDetails)
                    {
                        retVal = await _storeAdditionalInfocmiBL.UpdateBusinessDetailsInStoreAdditionalInfoCMI(storeAdditionalInfoCMI);
                    }
                    else if (storeAdditionalInfoCMI.SectionName == OnboardingScreenConstant.ShowroomDetails) 
                    {
                        retVal = await _storeAdditionalInfocmiBL.UpdateShowroomDetailsInStoreAdditionalInfoCMI(storeAdditionalInfoCMI);

                    }
                    else if (storeAdditionalInfoCMI.SectionName == OnboardingScreenConstant.EmployeeDetails)
                    {
                        retVal = await _storeAdditionalInfocmiBL.UpdateEmployeeDetailsInStoreAdditionalInfoCMI(storeAdditionalInfoCMI);

                    }
                    else if (storeAdditionalInfoCMI.SectionName == OnboardingScreenConstant.Karta)
                    {
                        retVal = await _storeAdditionalInfocmiBL.UpdateKartaInStoreAdditionalInfoCMI(storeAdditionalInfoCMI);

                    }
                    else if (storeAdditionalInfoCMI.SectionName == OnboardingScreenConstant.DistBusinessDetails)
                    {
                        retVal = await _storeAdditionalInfocmiBL.UpdateDistBusinessDetailsInStoreAdditionalInfoCMI(storeAdditionalInfoCMI);

                    }
                    else if (storeAdditionalInfoCMI.SectionName == OnboardingScreenConstant.AreaOfDistAgreed)
                    {
                        retVal = await _storeAdditionalInfocmiBL.UpdateAreaOfDistAgreedInStoreAdditionalInfoCMI(storeAdditionalInfoCMI);

                    }
                    else if (storeAdditionalInfoCMI.SectionName == OnboardingScreenConstant.AreaofOperationAgreed)
                    {
                        retVal = await _storeAdditionalInfocmiBL.UpdateAreaofOperationAgreedInStoreAdditionalInfoCMI(storeAdditionalInfoCMI);

                    }
                    else if (storeAdditionalInfoCMI.SectionName == OnboardingScreenConstant.BankersDetails)
                    {
                        retVal = await _storeAdditionalInfocmiBL.UpdateBankersDetailsInStoreAdditionalInfoCMI(storeAdditionalInfoCMI);

                    }
                    else if (storeAdditionalInfoCMI.SectionName == OnboardingScreenConstant.EarlierWorkWithCMI)
                    {
                        retVal = await _storeAdditionalInfocmiBL.UpdateEarlierWorkWithCMIInStoreAdditionalInfoCMI(storeAdditionalInfoCMI);

                    }
                    else if (storeAdditionalInfoCMI.SectionName == OnboardingScreenConstant.TermAndCond)
                    {
                        retVal = await _storeAdditionalInfocmiBL.UpdateTermAndCondInStoreAdditionalInfoCMI(storeAdditionalInfoCMI);
                    }
                    else if(storeAdditionalInfoCMI.SectionName==OnboardingScreenConstant.AreaofOperationAgreed)
                    {
                        retVal = await _storeAdditionalInfocmiBL.UpdateAreaofOperationAgreedInStoreAdditionalInfoCMI(storeAdditionalInfoCMI);
                    }
                    else if(storeAdditionalInfoCMI.SectionName==OnboardingScreenConstant.ServiceCenterDetail)
                    {
                        retVal = await _storeAdditionalInfocmiBL.UpdateServiceCenterDetailsInStoreAdditionalInfoCMI(storeAdditionalInfoCMI);
                    }
                    if (retVal > 0)
                    {
                        return CreateOkApiResponse(retVal);
                    }
                    else
                    {

                        throw new Exception("Update Failed");

                    }
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating Store Additional Info Details");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}
