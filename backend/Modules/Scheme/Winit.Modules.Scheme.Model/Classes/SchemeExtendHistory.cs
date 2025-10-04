using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Scheme.Model.Interfaces;

namespace Winit.Modules.Scheme.Model.Classes
{
    public class SchemeExtendHistory : BaseModel, ISchemeExtendHistory
    {
        public string SchemeType { get; set; }
        public string SchemeUid { get; set; }
        public string ActionType { get; set; }
        public DateTime? OldDate { get; set; }
        public DateTime? NewDate { get; set; }
        public string Comments { get; set; }
        public string UpdatedByEmpUid { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }
    }
}
