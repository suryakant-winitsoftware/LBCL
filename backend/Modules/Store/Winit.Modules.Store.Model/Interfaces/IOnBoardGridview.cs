using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Store.Model.Interfaces
{
    public interface IOnBoardGridview :IBaseModel
    {
        public string CustomerCode{get;set;}
        public string UID {get;set;}
        public string Status {get;set;}
        public string CustomerName { get;set;}
        public string BroadClassification{get;set;}
        public string OwnerName{get;set;}
        public bool IsApproved { get;set;}
    }
}
