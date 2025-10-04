
namespace Winit.Modules.Survey.Model.Interfaces
{
    public interface IViewSurveyResponse
    {
        string? CreatedDate { get; set; }
         TimeSpan? CreatedTime { get; set; }
         string? UserCode { get; set; }
         string? UserName { get; set; }
         string? CustomerCode { get; set; }
         string? CustomerName { get; set; }
         string? SurveyResponseUID { get; set; }
         string? SurveyName { get; set; }
         string? ImagePath { get; set; }
        string? RelativePathVideo { get; set; }
        string? ActivityType { get; set; }
         string? Status { get; set; }
         string? Question { get; set; }
         DateTime CreatedDateTime { get; set; }
        string? SurveyAge { get; set; }
        string? StatusExecutedorNot { get; set; }
        string? Users { get; set; }
        string? Stores_Customers { get; set; }
        public string Locationvalue { get; set; }
        public string LocationCode { get; set; }
        public string Role { get; set; }

    }

}
