using System.Text.Json.Serialization;
using Winit.Modules.Base.Model;
using static System.Collections.Specialized.BitVector32;

namespace Winit.Modules.Survey.Model.Interfaces
{
    public interface ISurveyResponseModel:IBaseModel
    {
         DateTime? ResponseDate { get; set; }
         string OrgUID { get; set; }
         string LinkedItemType { get; set; }
         string LinkedItemUID { get; set; }
         string JobPositionUid { get; set; }
         string EmpUID { get; set; }
         string RouteUID { get; set; }
         string StoreHistoryUID { get; set; }
         string BeatHistoryUID { get; set; }
         string SurveyUID { get; set; }
         string ActivityType { get; set; }
        string ResponseData { get; set; }
        string Status { get; set; }
        string Remarks { get;set; }

    }


}
