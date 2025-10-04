using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Winit.Modules.Store.Model.Interfaces
{
    public interface IStoreWeekOff : Base.Model.IBaseModel
    {
        public string StoreUID { get; set; }  
        public bool Sun { get; set; }  
        public bool Mon { get; set; }  
        public bool Tue { get; set; }  
        public bool Wed { get; set; }  
        public bool Thu { get; set; }  
        public bool Fri { get; set; }  
        public bool Sat { get; set; }  
    }
}
