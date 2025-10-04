using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.ListHeader.Model.Interfaces;
using Winit.Shared.Models.Common;
using Winit.Shared.Models.Enums;

namespace Winit.Modules.ListHeader.Model.Classes
{
    public class ListItem : BaseModel, IListItem
    {

        public string Code { get; set; }
        public string Name { get; set; }
        public bool IsEditable { get; set; }
        public int? SerialNo { get; set; }
        public string ListHeaderUID { get; set; }
    }

    public class ListItemRequest
    {

        public List<string> Codes { get; set; }
        public bool isCountRequired { get; set; }

    }
    public class ListItems
    {
       public ListItemRequest? ListItemRequest { get; set; }
       public PagingRequest? PagingRequest { get; set; }
    }
}
