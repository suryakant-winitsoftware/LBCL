using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Int_CommonMethods.Model.Interfaces;

namespace Winit.Modules.Int_CommonMethods.Model.Classes
{
    public class EntityDetails : BaseModel, IEntityDetails
    {
        public long SyncLogDetailId { get; set; }
        public string Entity { get; set; }
        public string TableName { get; set; }
        public string Action { get; set; }
        public string TablePrefix { get; set; }
        public DateTime LastSyncTimeStamp { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public int Sequence { get; set; }
    }
}
