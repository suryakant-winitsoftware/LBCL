using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Constants;
using Winit.Shared.Models.Enums;

namespace WINITAPI.Controllers.Contact
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : WINITBaseController
    {
        public ContactController() { }

        [HttpPost]
        [Route("SelectAllContactDetails")]
        public async Task<ActionResult> SelectAllContactDetails()
        {
            try
            {
                return CreateOkApiResponse<string>("Ramana");
            }
            catch (Exception)
            {
                throw;
            }
        }

       
    }
}
