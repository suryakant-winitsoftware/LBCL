using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Winit.Modules.Survey.Model.Interfaces
{
    public interface IActivityModule:IBaseModel
    {
        public DateTime Date { get; set; }
        public string outletname { get; set; } //StoreName
        public string outletcode { get; set; } //StorecodeERP
        public string UserName { get; set; }
        public string PoleSigange  { get; set; }
        public string FlangeSignage  { get; set; }
        public string Answer { get; set; } //Answers
        public string QuestionLabel { get; set; }//Questions
        public string AdditionalSignage { get; set; }
        public string Lightunderneathsignage { get; set; }
        public string Imageaddtionalsignage { get; set; }
        public string MainSignageReplacement { get; set; }
        public string MainSignageRemark { get; set; }
        public string HalogenLight { get; set; }
        public string Remarks { get; set; }
        public string Relativepath { get; set; } //AddImage
        public string Locationvalue { get; set; }
        public string LocationCode { get; set; }
        public string Role { get; set; }
    }
}
