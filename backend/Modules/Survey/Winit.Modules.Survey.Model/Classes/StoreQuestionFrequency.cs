using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Survey.Model.Interfaces;

namespace Winit.Modules.Survey.Model.Classes
{
    public class StoreQuestionFrequency : IStoreQuestionFrequency
    {
        public DateTime ResponseDate { get; set; }
        public string CustomerName { get; set; }
      public string CustomerCode { get; set; }
      public string UserName { get; set; }
      public string UserCode { get; set; }
      public string QuestionId { get; set; }
      public string Questions { get; set; }
      public long QuestionCount { get; set; }
        public string Locationvalue { get; set; }
        public string LocationCode { get; set; }
        public string Role { get; set; }

    }
}
