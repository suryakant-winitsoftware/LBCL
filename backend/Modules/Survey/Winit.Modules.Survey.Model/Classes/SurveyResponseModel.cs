using Newtonsoft.Json;
using Winit.Modules.Base.Model;
using Winit.Modules.Survey.Model.Interfaces;

namespace Winit.Modules.Survey.Model.Classes
{
    public class SurveyResponseModel :BaseModel, ISurveyResponseModel
    {
        public DateTime? ResponseDate { get; set; }
        public string OrgUID { get; set; }
        public string LinkedItemType { get; set; }
        public string LinkedItemUID { get; set; }
        public string JobPositionUid { get; set; }
        public string EmpUID { get; set; }
        public string RouteUID { get; set; }
        public string StoreHistoryUID { get; set; }
        public string BeatHistoryUID { get; set; }
        public string SurveyUID { get; set; }
        public string ActivityType { get; set; }
        public string ResponseData { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        [JsonIgnore]
        public Dictionary<string, List<string>>? RequestUIDDictionary { get; set; }
    }
}
