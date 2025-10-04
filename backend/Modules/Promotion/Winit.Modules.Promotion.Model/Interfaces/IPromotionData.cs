using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Promotion.Model.Interfaces
{
    public interface IPromotionData : IBaseModel
    {
        public string PromotionUID { get; set; }
        public string Data { get; set; }
        
     
    }

 }
