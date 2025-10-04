using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.ShareOfShelf.Model.Interfaces;

namespace Winit.Modules.ShareOfShelf.Model.Classes
{
    public class SosHeader :BaseModel ,ISosHeader
    {
        public string StoreUID { get; set; }
        public bool IsCompleted { get; set; }
        public string StoreHistoryUID { get; set; }
        public string BeatHistoryUID { get; set; }
        public string RouteUid { get; set; }
        public DateTime? ActivityDate { get; set; }
        public string JobPositionUID { get; set; }
        public string EmpUID { get; set; }
    }
}
