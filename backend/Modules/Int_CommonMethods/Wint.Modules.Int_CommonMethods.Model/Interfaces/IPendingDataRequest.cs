using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Int_CommonMethods.Model.Interfaces
{
    public interface IPendingDataRequest : IBaseModel
    {
        public string LinkedItemUid { get; set; }
        public string Status { get; set; } 
        public string LinkedItemType { get; set; }
    }
}
