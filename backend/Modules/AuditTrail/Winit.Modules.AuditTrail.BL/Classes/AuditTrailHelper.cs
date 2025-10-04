using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.AuditTrail.BL.Interfaces;
using Winit.Modules.AuditTrail.Model.Classes;
using Winit.Modules.Base.BL;

namespace Winit.Modules.AuditTrail.BL.Classes
{
    public class AuditTrailHelper : IAuditTrailHelper
    {
        private readonly ApiService _apiService;
        private readonly IConfiguration _configuration;
        public AuditTrailHelper(ApiService apiService, IConfiguration configuration)
        {
            _apiService = apiService;
            _configuration = configuration;
        }
        public async Task<bool> PublishAuditTrailEntry(IAuditTrailEntry auditTrailEntry)
        {
            try
            {
                var createResponse = await _apiService.FetchDataAsync<object>(
                    _configuration["AuditTrail:BaseURL"] + "PublishAuditTrail",
                    HttpMethod.Post,
                    auditTrailEntry
                );

                if (createResponse == null)
                {
                    throw new Exception("The API response is null.");
                }
                return true;
            }
            catch (Exception e)
            {
                throw new ApplicationException("An error occurred while processing the request.", e);
            }
        }
        public AuditTrailEntry CreateAuditTrailEntry(
        string linkedItemType, string linkedItemUID, string commandType,
        string docNo, string? jobPositionUID, string empUID, string empName,
        Dictionary<string, object> newData, string? originalDataId = null,
        List<ChangeLog>? changeData = null)
        {
            return new AuditTrailEntry
            {
                Id = Guid.NewGuid().ToString(),
                ServerCommandDate = DateTime.UtcNow,
                LinkedItemType = linkedItemType,
                LinkedItemUID = linkedItemUID,
                CommandType = commandType,
                CommandDate = DateTime.Now,
                DocNo = docNo,
                JobPositionUID = jobPositionUID,
                EmpUID = empUID,
                EmpName = empName,
                NewData = newData ?? new Dictionary<string, object>(),
                OriginalDataId = originalDataId,
                HasChanges = changeData != null && changeData.Count > 0,
                ChangeData = changeData
            };
        }
    }
}
