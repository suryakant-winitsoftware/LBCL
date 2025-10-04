using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Int_CommonMethods.Model.Interfaces;

namespace Winit.Modules.Int_CommonMethods.Model.Classes
{
    public class CommonStrings  
    {
        public  string TableMonthSuffix { get { return DateTime.Now.ToString("yyMM"); } }
        public string TableQueueSuffix { get { return "Queue"; } }
        public string? EntityName { get; set; }
        public string? EntityGroup { get; set; }
        public string? EntityType { get; set; }
        public string? TableName { get; set; }
        public Int64 SyncLogId { get; set; }
        public string UnderScore { get { return "_"; } }
        public string CreateTable
        {
            get { return "create table "; }
        }
    }
}
