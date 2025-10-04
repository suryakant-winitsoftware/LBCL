using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Winit.Modules.Scheme.Model.Interfaces;
using Winit.Modules.Store.BL.Interfaces;
using Winit.Modules.Store.Model.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WINITAPI.Controllers.Store
{
    [Route("api/[controller]")]
    [ApiController]
    public class SelfRegistrationController : WINITBaseController
    {
        private readonly Winit.Modules.Store.BL.Interfaces.ISelfRegistrationBL _selfRegistrationBL;
        public SelfRegistrationController(IServiceProvider serviceProvider, 
            Winit.Modules.Store.BL.Interfaces.ISelfRegistrationBL selfRegistrationBL) : base(serviceProvider)
        {
            _selfRegistrationBL = selfRegistrationBL;
        }
        [HttpPost("CrudSelfRegistration")]
        public async Task<IActionResult> CrudSelfRegistration([FromBody] Winit.Modules.Store.Model.Interfaces.ISelfRegistration selfRegistration)
        {
            try
            {
                if (selfRegistration == null)
                {
                    return BadRequest();
                }
                return await _selfRegistrationBL.CrudSelfRegistration(selfRegistration)
                    ? CreateOkApiResponse("Created Successfully")
                    : (IActionResult)CreateErrorResponse("Fail to Create SelfRegistration");
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
   
        [HttpGet("GetSelfRegistrationByUID")]
        public async Task<IActionResult> GetSelfRegistrationByUID([FromQuery] string UID)
        {
            try
            {
                if (string.IsNullOrEmpty(UID))
                {
                    return BadRequest();
                }
                Winit.Modules.Store.Model.Interfaces.ISelfRegistration selfRegistration = await _selfRegistrationBL.SelectSelfRegistrationByUID(UID);
                if (selfRegistration == null)
                {
                    _ = CreateErrorResponse("Error occured while retiving the SelfRegistration");
                }
                return CreateOkApiResponse(selfRegistration);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }

        [HttpPost("VerifyOtp")]
        public async Task<IActionResult> VerifyOtp([FromBody] ISelfRegistration selfRegistration)
        {
            try
            {
                if (selfRegistration == null || string.IsNullOrEmpty(selfRegistration.MobileNo) || string.IsNullOrEmpty(selfRegistration.UserEnteredOtp))
                {
                    return BadRequest("Invalid request.");
                }

                var existingRegistration = await _selfRegistrationBL.SelectSelfRegistrationByMobileNo(selfRegistration.MobileNo);

                bool isVerified;
                string uidToReturn;

                if (existingRegistration != null)
                {
                    isVerified = await _selfRegistrationBL.VerifyOTP(existingRegistration.UID, selfRegistration.UserEnteredOtp);

                    if (isVerified)
                    {
                        await _selfRegistrationBL.MarkOTPAsVerified(existingRegistration.UID);
                        uidToReturn = existingRegistration.UID; 
                        return CreateOkApiResponse(data: uidToReturn);
                    }
                    else
                    {
                        return CreateErrorResponse("Invalid OTP or OTP has already been verified.");
                    }
                }
                else
                {
                    isVerified = await _selfRegistrationBL.VerifyOTP(selfRegistration.UID, selfRegistration.UserEnteredOtp);

                    if (isVerified)
                    {
                        await _selfRegistrationBL.MarkOTPAsVerified(selfRegistration.UID);
                        uidToReturn = selfRegistration.UID;  
                        return CreateOkApiResponse(new { message = "OTP verified successfully.", UID = uidToReturn });
                    }
                    else
                    {
                        return CreateErrorResponse("Invalid OTP or OTP has already been verified.");
                    }
                }
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("An error occurred while verifying the OTP: " + ex.Message);
            }
        }

    }
}
