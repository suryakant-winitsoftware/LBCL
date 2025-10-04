using Microsoft.AspNetCore.Mvc;
using System;

namespace WINITAPI.Controllers.EmailController
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : WINITBaseController
    {
        private readonly Winit.Modules.Email.BL.Interfaces.IEmailBL _EmailBL;
        public EmailController(IServiceProvider serviceProvider,
            Winit.Modules.Email.BL.Interfaces.IEmailBL emailBL)
            : base(serviceProvider)
        {
            _EmailBL = emailBL;
        }
        //[HttpPost]
        //[Route("GetReceiverByTemplateForPO")]
        //public async Task<IActionResult> GetReceiverByTemplateForPO([FromBody] EmailFromBodyModelDTO model)
        //{
        //    try
        //    {
        //        int Result = 0;
        //        Result = await _EmailBL.GetReceiverByTemplateForPO(model);
        //        return Ok("Success");
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex, "Failed to insert mail request Details");
        //        return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
        //    }
        //}

    }
}
