using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.ListHeader.Model.Interfaces;


namespace Winit.Modules.ListHeader.Model.Classes
{
    public class ListHeader : BaseModel, IListHeader
    {
        public string CompanyUID { get; set; }
        public string OrgUID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsEditable { get; set; }
        public bool IsVisibleInUI { get; set; }
    }

   
}
