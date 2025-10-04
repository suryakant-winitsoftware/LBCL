using AuditTrailAPI3.BL;
using JsonDiffPatchDotNet;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using Winit.Modules.AuditTrail.BL.Classes;
using Winit.Modules.AuditTrail.BL.Interfaces;
using Winit.Modules.AuditTrail.Model.Classes;
using Winit.Modules.AuditTrail.Model.Constant;
using Winit.Modules.Notification.BL.Interfaces.Common;
using Winit.Shared.Models.Common;

namespace AuditTrailAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditTrailController : ControllerBase
    {
        private readonly IAuditTrailServiceBL _auditTrailService;
        private readonly AuditChangeProcessor _auditChangeProcessor;
        private readonly INotificationPublisherService _notificationPublisher;

        public AuditTrailController(IAuditTrailServiceBL auditTrailService,
            AuditChangeProcessor auditChangeProcessor,
            INotificationPublisherService notificationPublisher)
        {
            _auditTrailService = auditTrailService;
            _auditChangeProcessor = auditChangeProcessor;
            _notificationPublisher = notificationPublisher;
        }
        [HttpPost("CreateAuditTrailTest")]
        public async Task<IActionResult> CreateAuditTrailTest([FromBody] AuditTrailEntry auditTrailEntry)
        {
            if (auditTrailEntry == null)
            {
                return BadRequest("AuditTrail entry is null.");
            }
            auditTrailEntry.HasChanges = false;
            auditTrailEntry.NewData = AuditTrailCommonFunctions.ConvertToBsonDeserializedData<Dictionary<string, object>>(auditTrailEntry.NewData);

            // Get Original Data
            AuditTrailEntry auditTrailEntryOriginal = await _auditTrailService.GetLastAuditTrailAsync(auditTrailEntry.LinkedItemType, auditTrailEntry.LinkedItemUID, false);

            // Find Track changes
            string originalDataSerialized = "{}";
            if (auditTrailEntryOriginal != null)
            {
                auditTrailEntry.OriginalDataId = auditTrailEntryOriginal.Id;
                originalDataSerialized = JsonSerializer.Serialize(auditTrailEntryOriginal.NewData);

                string newDataSerialized = JsonSerializer.Serialize(auditTrailEntry.NewData);

                var jdp = new JsonDiffPatch();
                var diff = jdp.Diff(originalDataSerialized, newDataSerialized);
                if (diff != null)
                {
                    List<ChangeLog>? changeLogs = null;
                    changeLogs = _auditChangeProcessor.ExtractChanges(JObject.Parse(diff), changeLogs);
                    auditTrailEntry.ChangeData = changeLogs;
                    auditTrailEntry.HasChanges = true;
                }
            }

            // Update to DB
            await _auditTrailService.CreateAuditTrailAsync(auditTrailEntry);
            return Ok("AuditTrail entry created successfully.");
        }

        // GET: api/AuditTrail/{entityName}/{entityId}
        [HttpGet("{linkedItemType}/{linkedItemUID}")]
        public async Task<IActionResult> GetAuditTrails(string linkedItemType, string linkedItemUID)
        {
            var auditTrails = await _auditTrailService.GetAuditTrailsAsync(linkedItemType, linkedItemUID);
            if (auditTrails == null || auditTrails.Count == 0)
            {
                return NotFound("No audit trails found for the specified entity.");
            }

            return Ok(auditTrails);
        }
        [HttpGet("GetAuditTrailByIdAsync")]
        public async Task<IActionResult> GetAuditTrailByIdAsync(string id, bool isChangeDataRequired = false)
        {
            var auditTrails = await _auditTrailService.GetAuditTrailByIdAsync(id, isChangeDataRequired);
            if (auditTrails == null)
            {
                return NotFound("No audit trails found for the given Id.");
            }

            return Ok(auditTrails);
        }
        // GET: api/AuditTrail/{entityName}/{entityId}
        [HttpPost("GetAuditTrailsAsyncByPaging")]
        public async Task<IActionResult> GetAuditTrailsAsyncByPaging([FromBody] PagingRequest pagingRequest)
        {
            if (pagingRequest == null)
            {
                return BadRequest("Invalid request data");
            }
            if (pagingRequest.PageNumber < 0 || pagingRequest.PageSize < 0)
            {
                return BadRequest("Invalid page size or page number");
            }
            var pagedResponse = await _auditTrailService.GetAuditTrailsAsyncByPaging(pagingRequest.SortCriterias,
                   pagingRequest.PageNumber, pagingRequest.PageSize, pagingRequest.FilterCriterias,
                   pagingRequest.IsCountRequired);
            if (pagedResponse == null)
            {
                return NotFound("No audit trails found for the specified entity.");
            }

            return Ok(pagedResponse);
        }

        // PUT: api/AuditTrail/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAuditTrail(string id, [FromBody] AuditTrailEntry updatedAuditTrailEntry)
        {
            if (updatedAuditTrailEntry == null)
            {
                return BadRequest("Updated audit trail entry is null.");
            }

            await _auditTrailService.UpdateAuditTrailAsync(id, updatedAuditTrailEntry);
            return Ok("AuditTrail entry updated successfully.");
        }

        // DELETE: api/AuditTrail/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuditTrail(string id)
        {
            await _auditTrailService.DeleteAuditTrailAsync(id);
            return Ok("AuditTrail entry deleted successfully.");
        }

        private string ValidatePublishAuditTrail(AuditTrailEntry auditTrailEntry)
        {
            string errorMessage = string.Empty;
            if (auditTrailEntry == null)
            {
                errorMessage = "Request should not be null";
            }
            else if (auditTrailEntry.NewData == null)
            {
                errorMessage = "NewData is null.";
            }
            else if (string.IsNullOrEmpty(auditTrailEntry.LinkedItemType))
            {
                errorMessage = "LinkedItemType is mandatory.";
            }
            else if (string.IsNullOrEmpty(auditTrailEntry.LinkedItemUID))
            {
                errorMessage = "LinkedItemUID is mandatory.";
            }
            else if (string.IsNullOrEmpty(auditTrailEntry.CommandType))
            {
                errorMessage = "CommandType is mandatory.";
            }            
            else if (string.IsNullOrEmpty(auditTrailEntry.EmpUID))
            {
                errorMessage = "EmpUID is mandatory.";
            }
            else if (string.IsNullOrEmpty(auditTrailEntry.EmpName))
            {
                errorMessage = "EmpName is mandatory.";
            }
            else if (auditTrailEntry.NewData == null)
            {
                errorMessage = "NewData is null.";
            }
            return errorMessage;
        }
        [HttpPost("PublishAuditTrail")]
        public async Task<IActionResult> PublishAuditTrail([FromBody] AuditTrailEntry auditTrailEntry)
        {
            try
            {
                string errorMessage = ValidatePublishAuditTrail(auditTrailEntry);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    return BadRequest(errorMessage);
                }
                auditTrailEntry.NewData = AuditTrailCommonFunctions.ConvertToBsonDeserializedData<Dictionary<string, object>>(auditTrailEntry.NewData);
                await _notificationPublisher.PublishToTopicExchange((IAuditTrailEntry)auditTrailEntry, QueueRoute.AuditTrail_General);

                return Ok(true);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

    }

}
