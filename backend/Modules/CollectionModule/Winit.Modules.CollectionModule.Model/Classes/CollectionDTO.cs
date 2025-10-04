
using Newtonsoft.Json;
using Winit.Modules.CollectionModule.Model.Interfaces;

namespace Winit.Modules.CollectionModule.Model.Classes;

public class CollectionDTO : AccCollection
{
    public AccCollection AccCollection { get; set; }
    public AccCollectionPaymentMode AccCollectionPaymentMode { get; set; }
    public AccStoreLedger AccStoreLedger { get; set; }
    public AccCollectionSettlement AccCollectionSettlement { get; set; }
    public List<AccCollectionAllotment> AccCollectionAllotment { get; set; }
    public List<AccPayable> AccPayable { get; set; }
    public List<AccReceivable> AccReceivable { get; set; }
    public List<AccCollectionSettlementReceipts> AccCollectionSettlementReceipts { get; set; }
    public List<AccCollectionCurrencyDetails> AccCollectionCurrencyDetails { get; set; }
    public Winit.Modules.JourneyPlan.Model.Classes.StoreHistory? StoreHistory { get; set; }

    [JsonIgnore]
    public Dictionary<string, List<string>>? RequestUIDDictionary { get; set; }
}
