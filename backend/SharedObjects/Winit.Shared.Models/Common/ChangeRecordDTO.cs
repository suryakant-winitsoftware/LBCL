using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Shared.Models.Common
{
    public class ChangeRecordDTO : IChangeRecordDTO
    {
        public string LinkedItemUID { get; set; }
        public string Action {  get; set; }
        public string ScreenModelName { get; set; }
        public string UID { get; set; }
        public List<ChangeRecords> ChangeRecords { get; set; }

    }

}
