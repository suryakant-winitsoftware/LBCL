
using Winit.Modules.Survey.Model.Classes;

namespace Winit.Modules.Survey.Model.Interfaces
{
    public interface ISurveyResponseViewDTO
    {
     //   string Category { get; set; }
        List<QuestionAnswer> QuestionAnswers { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCode { get; set; }
    }
}
