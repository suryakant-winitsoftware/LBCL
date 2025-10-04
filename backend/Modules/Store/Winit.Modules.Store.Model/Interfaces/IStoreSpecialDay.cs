using System;
using System.Collections.Generic;
using System.Text;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Store.Model.Interfaces
{
    public interface IStoreSpecialDay:IBaseModel
    {
        public string StoreUID { get; set; }
    }
}
