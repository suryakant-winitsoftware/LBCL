using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIModels.Web.Store
{
    public  class ListitemModel : BaseModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsEditable { get; set; }
        public int SerialNo { get; set; }
        public string ListHeaderUID { get; set; }
    }
}
