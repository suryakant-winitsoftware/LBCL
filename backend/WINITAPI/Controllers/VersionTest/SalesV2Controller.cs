using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WINITAPI.Controllers.VersionTest
{
    [ApiController]
    [Route("api/v2.0/[controller]")]
    [ApiVersion("2.0")]
    [Authorize]
    public class SalesV2Controller : ControllerBase
    {
        [HttpGet("Method2")]
        public async new Task<ActionResult<IEnumerable<Winit.Modules.Store.Model.Interfaces.IStore>>> Method2()
        {
            // Version 2 logic (new method)
            return Ok("version2 Method2");
        }
        [HttpGet("Method3")]
        public async Task<ActionResult<IEnumerable<Winit.Modules.Store.Model.Interfaces.IStore>>> Method3()
        {
            // Version 2 logic (new method)
            return Ok("version2 Method3");
        }
    }
}
