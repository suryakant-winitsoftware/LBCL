using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Merchandiser.Model.Classes;
using Winit.Modules.Planogram.Model.Classes;
using Winit.Modules.PO.Model.Classes;
using Winit.Modules.StoreCheck.Model.Classes;
using Winit.Modules.ExpiryCheck.Model.Classes;

namespace Winit.Modules.CaptureCompetitor.Model.Classes
{
    public class MerchandiserDTO
    {
        public List<CaptureCompetitor> captureCompetitor { get; set; }
        public List<CategoryBrandMapping> categoryBrandMapping { get; set; }
        public List<CategoryBrandCompetitorMapping> categoryBrandCompetitorMapping { get; set; }
        public List<PlanogramSetup> planogramSetup { get; set; }
        public List<PlanogramExecutionHeader> planogramExecutionHeader { get; set; }
        public List<PlanogramExecutionDetail> planogramExecutionDetails { get; set; }
        public List<StoreCheckGroupHistory>? storeCheckGroupHistory { get; set; }
        public List<StoreCheckItemHistory>? storeCheckItemHistory { get; set; }
        public List<StoreCheckItemUomQty>? storeCheckItemUom { get; set; }
        public List<StoreCheckHistoryView>? storeCheckHistory { get; set; }
        public List<StoreCheckItemExpiryDREHistory>? storeCheckExpiryDREHistory { get; set; }
        public List<POExecution>? poExecution { get; set; }
        public List<POExecutionLine>? poExecutionLine { get; set; }
        public List<ProductFeedback>? productFeedback { get; set; }
        public List<BroadcastInitiative>? broadcastInitiative { get; set; }
        public List<ExpiryCheckExecution>? expiryCheckExecution { get; set; }
        public List<ExpiryCheckExecutionLine>? expiryCheckExecutionLine { get; set; }
        public List<PlanogramExecutionV1>? planogramExecutionV1 { get; set; }
        public List<ProductSampling>? productSampling { get; set; }
        [JsonIgnore]
        public Dictionary<string, List<string>>? RequestUIDDictionary { get; set; }
    }
}