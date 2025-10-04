using Winit.Modules.Survey.Model.Interfaces;

namespace Winit.Modules.Survey.Model.Classes
{
    public class ViewSurveyResponse : IViewSurveyResponse
    {
        public string? CreatedDate { get; set; }  
        public TimeSpan? CreatedTime { get; set; }  
        public string? UserCode { get; set; }       
        public string? UserName { get; set; }      
        public string? CustomerCode { get; set; }   
        public string? CustomerName { get; set; }   
        public string? SurveyResponseUID { get; set; }
        public string? SurveyName { get; set; }
        public string? ImagePath { get; set; }
        public string? RelativePathVideo { get; set; }
        public string? ActivityType { get; set; }
       public string? Status { get; set; }
       public string? Question { get; set; }
      public  DateTime CreatedDateTime { get; set; }
        public string? SurveyAge { get; set; }
        public string? StatusExecutedorNot { get; set; }
        public string? Users { get; set; }
        public string? Stores_Customers { get; set; }
        public string Locationvalue { get; set; }
        public string LocationCode { get; set; }
        public string Role { get; set; }

    }

}
