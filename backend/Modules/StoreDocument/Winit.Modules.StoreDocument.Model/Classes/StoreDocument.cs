using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.StoreDocument.Model.Interfaces;

namespace Winit.Modules.StoreDocument.Model.Classes
{
    public class StoreDocument:BaseModel,IStoreDocument
    {
        public string StoreUID { get; set; }
        public string DocumentType { get; set; }
        public string? DocumentLabel { get; set; }
        public string DocumentNo { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUpTo { get; set; }
        public bool IsNewDoc {  get; set; }
    }
}
