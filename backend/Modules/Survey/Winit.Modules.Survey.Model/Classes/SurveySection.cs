using Newtonsoft.Json;
using Winit.Modules.Base.Model;
using Winit.Modules.Survey.Model.Interfaces;

namespace Winit.Modules.Survey.Model.Classes
{
    public class SurveySection : BaseModel , ISurveySection
    {
        [JsonProperty("survey_data")]
        public string SurveyData { get; set; }
    }
}
