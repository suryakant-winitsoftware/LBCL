using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Winit.Modules.SMS.Model.Classes;
using Winit.Modules.SMS.Model.Interfaces;

namespace WINITAPI.Controllers.SMSController
{
    [Route("api/[controller]")]
    [ApiController]
    public class SMSController : WINITBaseController
    {
        private readonly Winit.Modules.SMS.BL.Interfaces.ISMSBL _SMSBL;
        public SMSController(IServiceProvider serviceProvider,
            Winit.Modules.SMS.BL.Interfaces.ISMSBL sMSBL) : base(serviceProvider)
        {
            _SMSBL = sMSBL;
        }

        [HttpPost]
        [Route("GetReceiverByTemplateForPO")]
        public async Task<IActionResult> GetReceiverByTemplateForPO([FromBody] SmsFromBodyModelDTO model)
        {
            //try
            //{
            //    int Result = 0;
            //    Result = await _SMSBL.GetReceiverByTemplateForPO(model);
            //    return Ok("Success");
            //}
            //catch (Exception ex)
            //{
            //    throw;
            //}
            return default;
        }
        [HttpGet]
        [Route("GetSMSContentByTemplateForPO")]
        public async Task<IActionResult> GetSMSContentByTemplateForPO([FromQuery] string templateName, [FromBody] SmsTemplateFields smsTemplateFields)
        {
            try
            {
                //SmsTemplates smsTemplate = (SmsTemplates)Enum.Parse(typeof(SmsTemplates), templateName);
                //List<SmsRequestModel> ReceiverTemplateDetails = await _SMSBL.GetSMSContentByTemplateForPO(smsTemplate, smsTemplateFields);
                //if (ReceiverTemplateDetails.Count == 0)
                //{
                //    return NotFound("No data found");
                //}
                //else
                //{
                //    return Ok();
                //}
                return default;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("CreateSmsRequest")]
        public async Task<IActionResult> CreateSmsRequest([FromBody] SmsRequestModel model)
        {
            try
            {
                //string GUID = Guid.NewGuid().ToString();
                //if (model.Receivers == null || !model.Receivers.Any())
                //{
                //    return BadRequest(new { Message = "At least one receiver is required." });
                //}
                //model.UID = GUID;
                //int result = await _SMSBL.CreateSmsRequest(model);
                //if (result != 0)
                //{
                //    return Ok(new SmsResponseModel
                //    {
                //        RequestID = GUID,
                //        Status = "Success",
                //        Message = "SMS request created successfully."
                //    });
                //}
                //return BadRequest("SMS request failed");
                return default;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        [HttpGet]
        [Route("GetSmsRequest")]
        public async Task<IActionResult> GetSmsRequest([FromQuery] string UID)
        {
            try
            {
                if (UID == null)
                    return NotFound(new { Message = "UID cannot be null" });
                var SmsDetails = await _SMSBL.GetSmsRequest(UID);
                if (SmsDetails.Count == 0)
                {
                    return NotFound(new { Message = "SMS request not found." });
                }

                return Ok(SmsDetails);
            }
            catch (Exception ex)
            {
                return NotFound(new { Message = "Exception" });
            }
        }
        [HttpPost]
        [Route("SendOtp")]
        public async Task<SMSApiResponse> SendOtp(ISms smsRequest)
        {
            try
            {
                SMSApiResponse smsResponse = await _SMSBL.SendOtp(smsRequest);
                return smsResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
