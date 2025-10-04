using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.Survey.Model.Classes
{
    public class CreateSurveyConstants
    {
        public static class DropDownTypes
        {
            public const string Text = "text";
            public const string Radio = "radio";
            public const string Image = "Image";
            public const string Video = "Video";
            public const string StarRating = "star-rating";
            public const string Checkbox = "checkbox";
            public const string Dropdown = "dropdown";
            public const string DateTime = "datetime";
            public const string MultiDropdown = "MultiDropdown";
        }

        public static class DateTypes
        {
            public const string Today = "Today";
            public const string Yesterday = "Yesterday";
            public const string SpecificDate = "Specificdate";
        }
        public static class QuestionName
        {
            public const string Question1 = "Question 1";
        }
    }
}
