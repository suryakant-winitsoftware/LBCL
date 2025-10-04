using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManagerModel.Interfaces
{
    public interface IEntityScript
    {
        public string EntityName { get; set; }
        public string EntityGroup { get; set; }
        public string SelectQuery { get; set; }
        public string InsertQuery { get; set; }
        public int MaxCount { get; set; }
    }
}
