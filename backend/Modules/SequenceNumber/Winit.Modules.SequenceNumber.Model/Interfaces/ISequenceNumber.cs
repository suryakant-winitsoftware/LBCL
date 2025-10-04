using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.SequenceNumber.Model.Interfaces
{
    public interface ISequenceNumber : IBaseModel
    {
        public string ModuleName { get; set; }
        public string JobPositionSeqCode { get; set; }
        public string OrgSeqCode { get; set; }
        public string TypeSeqCode { get; set; }
        public int DigitPadding { get; set; }
        public int CurrentSequenceNumber { get; set; }
    }
}
