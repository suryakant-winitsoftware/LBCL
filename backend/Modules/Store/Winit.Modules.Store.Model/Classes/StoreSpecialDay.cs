using System;
using System.Collections.Generic;
using System.Text;
using Winit.Modules.Base.Model;
using Winit.Modules.Store.Model.Interfaces;

namespace Winit.Modules.Store.Model.Classes
{
    public class StoreSpecialDay : BaseModel, IStoreSpecialDay
    {
        public string StoreUID { get; set; }
    }
}
