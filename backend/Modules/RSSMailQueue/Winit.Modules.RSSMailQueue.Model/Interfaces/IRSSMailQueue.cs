using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.RSSMailQueue.Model.Interfaces
{
    public interface IRSSMailQueue:IBaseModel
    {
        string LinkedItemType{ get; set; }
        string LinkedItemUID{ get; set; }
        int MailStatus{ get; set; }
        string Type{ get; set; }
        string Subject{ get; set; }
        string Body{ get; set; }
        string FromMail{ get; set; }
        string CCMail{ get; set; }
        string ToMail{ get; set; }
        bool HasAttachment{ get; set; }
        string  AttachmentFormatCode{ get; set; }
        string  FormatCode{ get; set; }
        string  ErrorMessage{ get; set; }
    }
}
