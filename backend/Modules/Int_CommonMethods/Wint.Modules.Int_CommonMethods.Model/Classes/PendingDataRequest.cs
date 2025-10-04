using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Int_CommonMethods.Model.Interfaces;

namespace Winit.Modules.Int_CommonMethods.Model.Classes
{
    public class PendingDataRequest: BaseModel,IPendingDataRequest
    {
        public string LinkedItemUid { get; set; }
        public string Status { get; set; }
        public string LinkedItemType { get; set; }

    }
}
