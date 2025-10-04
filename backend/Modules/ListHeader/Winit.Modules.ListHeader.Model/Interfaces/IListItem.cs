using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.ListHeader.Model.Interfaces
{
    public interface IListItem: IBaseModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsEditable { get; set; }
        public int? SerialNo { get; set; }
        public string ListHeaderUID { get; set; }
    }
}
