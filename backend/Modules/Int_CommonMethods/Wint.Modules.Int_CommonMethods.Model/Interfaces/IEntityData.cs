using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Int_CommonMethods.Model.Interfaces
{
    public interface IEntityData : IBaseModel
    {
        public string EntityName { get; set; }
        public string EntityGroup { get; set; }
        public string SelectQuery  { get; set; }
        public string InsertQuery { get; set; }
        public string MaxCount { get; set; }

    }
}
