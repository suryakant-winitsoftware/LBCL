using System.Text.Json.Serialization;
using Winit.Modules.Survey.Model.Interfaces;

namespace Winit.Modules.Survey.Model.Classes
{
    public class SurveyResponseViewDTO : ISurveyResponseViewDTO
    {
        //public string Category { get; set; }
        public List<QuestionAnswer> QuestionAnswers { get; set; }
        public string CustomerName { get; set; }
        public string CustomerCode { get; set; }
    }

    //using below classes for mapping from josn objects
    public class Section
    {
        [JsonPropertyName("section_id")]
        public string SectionId { get; set; }
        [JsonPropertyName("section_title")]
        public string SectionTitle { get; set; }
        [JsonPropertyName("seq_no")]
        public int SeqNo { get; set; }
        [JsonPropertyName("questions")]
        public List<Question> Questions { get; set; }
    }
    public class SurveyData
    {
        [JsonPropertyName("survey_id")]
        public string SurveyId { get; set; }
        public string Title { get; set; }
        [JsonPropertyName("sections")]
        public List<Section> Sections { get; set; }
    }
    public class Question
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("label")]
        public string Label { get; set; }
        [JsonPropertyName("seq_no")]
        public int SeqNo { get; set; }
        [JsonPropertyName("required")]
        public bool Required { get; set; }
        [JsonPropertyName("camera_visible")]
        public bool CameraVisible { get; set; }
        [JsonPropertyName("options")]
        public List<Option> Options { get; set; }
    }
    public class Option
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }
        [JsonPropertyName("points")]
        public int Points { get; set; }
    }

    public class ResponseData
    {
        [JsonPropertyName("responses")]
        public List<Response> Responses { get; set; }

        [JsonPropertyName("section_id")]
        public string SectionId { get; set; }

        [JsonPropertyName("submission_time")]
        public DateTime SubmissionTime { get; set; }
    }

    public class Response
    {
        [JsonPropertyName("question_id")]
        public string QuestionId { get; set; }
        [JsonPropertyName("question_label")]
        public string QuestionLabel { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("values")]
        public List<string> Values { get; set; }

        [JsonPropertyName("points")]
        public int Points { get; set; }
        [JsonPropertyName("activity_type")]
        public string ActivityType { get; set; }
    }


    public class QuestionAnswer
    {
        public string Category { get; set; }
        public string Question { get; set; }
        public string CustomerName { get; set; }
        public string Value { get; set; }
        public List<string> Values { get; set; }
        public int Points { get; set; }
        public string ActivityType { get; set; }
        public string RelativePath { get; set; }
        public string RelativePathVideo { get; set; }
        public string RelativePathRaiseTkt { get; set; }
        public string ImagePath { get; set; }
    }
    public class SurveyResponseResult
    {
        public string SurveyData { get; set; }
        public string ResponseData { get; set; }
        public string LinkedItemUID { get; set; }
        public string RelativePath { get; set; }
        public string RelativePathVideo { get; set; }
        public string RelativePathRaiseTkt { get; set; }         
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
    }


}
