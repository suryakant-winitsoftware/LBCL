using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Winit.Modules.AuditTrail.BL.Interfaces;
using Winit.Modules.AuditTrail.BL.Classes;
using Winit.Modules.AuditTrail.Model.Classes;
using JsonDiffPatchDotNet;
using AuditTrailConsumer.BL;
using Newtonsoft.Json.Linq;

namespace AuditTrailConsumer.Consumers
{
    public class AuditTrailProcessConsumer : IConsumer<AuditTrailEntry>
    {
        private readonly IAuditTrailServiceBL _auditTrailServiceBL;
        private readonly AuditChangeProcessor _auditChangeProcessor;
        public AuditTrailProcessConsumer(IAuditTrailServiceBL auditTrailServiceBL,
            AuditChangeProcessor auditChangeProcessor)
        {
            _auditTrailServiceBL = auditTrailServiceBL;
            _auditChangeProcessor = auditChangeProcessor;
        }

        public async Task Consume(ConsumeContext<AuditTrailEntry> context)
        {
            AuditTrailEntry auditTrailEntry = context.Message;
            try
            {
                if (auditTrailEntry == null)
                {
                    return;
                }
                auditTrailEntry.HasChanges = false;
                //auditTrailEntry.NewData = AuditTrailCommonFunctions.ConvertToBsonDeserializedData<Dictionary<string, object>>(auditTrailEntry.NewData);

                // Get Original Data
                AuditTrailEntry auditTrailEntryOriginal = await _auditTrailServiceBL.GetLastAuditTrailAsync(auditTrailEntry.LinkedItemType, auditTrailEntry.LinkedItemUID, false);

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
                await _auditTrailServiceBL.CreateAuditTrailAsync(auditTrailEntry);
            }
            catch (Exception ex)
            {
                // Log Error in file [AuditTrailErrorLog]
                throw;
            }
        }
    }
}
