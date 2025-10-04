using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Winit.Modules.Calender.BL.Interfaces;
using Winit.Modules.Calender.Models.Classes;

namespace WINITAPI.Controllers.Calender
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalenderController : WINITBaseController
    {
        ICalenderBL _calenderBL { get; }
        public CalenderController(IServiceProvider serviceProvider, 
            ICalenderBL calenderBL) : base(serviceProvider)
        {
            _calenderBL = calenderBL;
        }

        [HttpPost]
        [Route("GetCalenderPeriods")]
        public async Task<ActionResult> GetCalenderPeriods([FromBody] CalenderPeriodRequest calenderPeriodRequest)
        {
            if (calenderPeriodRequest == null)
            {
                return BadRequest();
            }
            try
            {
                var period = await _calenderBL.GetCalenderPeriods(calenderPeriodRequest.Date, calenderPeriodRequest.Period);
                if (period != null)
                {
                    return CreateOkApiResponse(period);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, $"Failed to retrieve Calender Periods with Calender Period: {calenderPeriodRequest.Period} and Date :{calenderPeriodRequest.Date}");
                return CreateErrorResponse("An error occurred while processing the request." + ex.Message);
            }
        }
    }
}
