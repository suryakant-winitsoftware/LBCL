using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Location.Model.Interfaces
{
    public interface ILocationType:IBaseModel
    {
       
        public string CompanyUID { get; set; }
        public string Name { get; set; }
        public string ParentUID { get; set; }
        public string Code { get; set; }
        public int LevelNo { get; set; }
        public bool ShowInUI { get; set; }
        public bool? ShowInTemplate { get; set; }
    }
}
