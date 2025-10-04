using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Int_CommonMethods.Model.Interfaces
{
    public interface IEntityDetails :IBaseModel
    {
        public long SyncLogDetailId { get; set; }
        public string Entity { get; set; }
        public string TableName { get; set; }
        public string TablePrefix { get; set; }
        public string Action { get; set; }
        public DateTime LastSyncTimeStamp { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public int Sequence { get; set; }
    }
}
