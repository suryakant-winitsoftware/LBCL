using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Survey.Model.Interfaces;
using Winit.Shared.Models.Common;
using JsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;

namespace Winit.Modules.Survey.Model.Classes
{
    public class ManageSurvey : IManageSurvey
    {
        [JsonProperty("survey_id")]
        public string SurveyId { get; set; }
        [JsonProperty("Code")]
        public string Code { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("start_date")]
        public string StartDate { get; set; }

        [JsonProperty("end_date")]
        public string EndDate { get; set; }

        [JsonProperty("is_active")]
        public bool IsActive { get; set; }

        [JsonProperty("sections")]
        public List<ManageSection> Sections { get; set; } = new();
    }

    public class ManageSection : IManageSection
    {
        [JsonProperty("section_id")]
        public string SectionId { get; set; }

        [JsonProperty("section_title")]
        public string SectionTitle { get; set; }

        [JsonProperty("seq_no")]
        public int SeqNo { get; set; }

        [JsonProperty("questions")]
        public List<ManageQuestion> Questions { get; set; } = new();
    }

    public class ManageQuestion : IManageQuestion
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public string LabelQuestion { get; set; }
        public bool IsScoreRequired { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]

        public bool IsDateRequired { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]

        public bool IsTimeRequired { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]

        public string MinDate { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? MinSpecificDate { get; set; }
        public string MaxDate { get; set; }
        public string? MaxSpecificDate { get; set; }
        public bool IsRequired { get; set; }
        public bool IsCameraVisible { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("seq_no")]
        public int SeqNo { get; set; }

        [JsonProperty("options")]
        public List<ManageOption> Options { get; set; } = new();
        [JsonIgnore]
        public List<ISelectionItem> SelectionItems { get; set; } = new();
        [JsonIgnore]
        public List<ISelectionItem> MinDateSelectionItems { get; set; } = new();
        [JsonIgnore]
        public List<ISelectionItem> MaxDateSelectionItems { get; set; } = new();
        
        public Validations Validations { get; set; } = new Validations(); // Add this property for validations

    }

    public class ManageOption : IManageOption
    {
        [JsonProperty("label")]
        public string Label { get; set; }
        public string LabelOption { get; set; }

        [JsonProperty("points")]
        public int Points { get; set; }
    }

    public class Validations
    {
        [JsonProperty("is_mandatory")]
        public bool is_mandatory { get; set; }
    }

}
