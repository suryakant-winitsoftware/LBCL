using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.Model.Classes
{ 
    public class StoreToGroupMapping:BaseModel, IStoreToGroupMapping
    {
     
        public string StoreGroupUID { get; set; }
        public string StoreUID { get; set; }
       
    }
}
