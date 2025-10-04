using Winit.Modules.Survey.Model.Interfaces;

namespace Winit.Modules.Survey.Model.Classes
{
    public class SurveyResponse : ISurveyResponse
    {
        public string Id { get; set; }
        public string Question { get; set; }
        public string SectionID { get; set; }
        public string Type { get; set; }
        public string Value { get; set; } 
        public List<string> Values { get; set; } = new(); 
        public int Points { get; set; }
    }

}
