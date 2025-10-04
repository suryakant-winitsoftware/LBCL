using Newtonsoft.Json;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Survey.Model.Interfaces
{
    public interface ISurveySection : IBaseModel
    {
        [JsonProperty("survey_data")]
        public string SurveyData { get; set; }
    }
}
