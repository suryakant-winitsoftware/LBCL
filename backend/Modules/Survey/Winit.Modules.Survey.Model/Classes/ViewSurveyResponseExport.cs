using Winit.Modules.Survey.Model.Interfaces;

namespace Winit.Modules.Survey.Model.Classes
{
    public class ViewSurveyResponseExport : ViewSurveyResponse, IViewSurveyResponseExport
    {
     
        public string Category { get; set; }
        public string Question { get; set; }
        public string Value { get; set; }
        public string RelativePath { get; set; }
        public string RelativePathRaiseTkt { get; set; }
        public List<string> Values { get; set; }
        public int Points { get; set; }
        public string Status { get; set; }


    }

}
