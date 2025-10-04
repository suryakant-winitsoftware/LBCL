using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.StoreDocument.Model.Interfaces
{
    public interface IStoreDocument : IBaseModel
    {
        string StoreUID { get; set; }
        string DocumentType { get; set; }
        string? DocumentLabel { get; set; }
        string DocumentNo { get; set; }
        DateTime? ValidFrom { get; set; }
        DateTime? ValidUpTo { get; set; }
        bool IsNewDoc { get; set; }
    }
}
