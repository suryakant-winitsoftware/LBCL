using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Survey.Model.Classes;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Survey.Model.Interfaces
{
    public interface IStoreQuestionFrequency
    {
        DateTime ResponseDate { get; set; }
        string CustomerName { get; set; }
        string CustomerCode { get; set; }
        string UserName { get; set; }
        string UserCode { get; set; }
        string QuestionId { get; set; }
        string Questions { get; set; }
        long QuestionCount { get; set; }
        public string Locationvalue { get; set; }
        public string LocationCode { get; set; }
        public string Role { get; set; }
    }

}
