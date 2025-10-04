using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.UIModels.Web.Store
{
    public class DocumentModel : BaseModel
    {
        public string StoreUID { get; set; }
        public string DocumentType { get; set; }
        public string DocumentNo { get; set; }
        public string DocImage { get; set; }
       
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUpTo { get; set; }
    }
}
