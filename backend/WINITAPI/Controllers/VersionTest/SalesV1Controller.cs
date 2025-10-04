using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WINITAPI.Controllers.VersionTest
{
    [ApiController]
    [Route("api/v1.0/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class SalesV1Controller : ControllerBase
    {
        [HttpGet("Method1")]
        public async Task<ActionResult<IEnumerable<Winit.Modules.Store.Model.Interfaces.IStore>>> Method1()
        {
            // Version 2 logic (new method)
            return Ok("version1 Method1");
        }

        [HttpGet("Method2")]
        public async Task<ActionResult<IEnumerable<Winit.Modules.Store.Model.Interfaces.IStore>>> Method2()
        {
            // Version 2 logic (new method)
            return Ok("version1 Method2");
        }
    }
}
