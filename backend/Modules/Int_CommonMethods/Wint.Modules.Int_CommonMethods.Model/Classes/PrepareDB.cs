using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;
using Winit.Modules.Int_CommonMethods.Model.Interfaces;

namespace Winit.Modules.Int_CommonMethods.Model.Classes
{
    public class PrepareDB :BaseModel, IPrepareDB
    {
        public string Script { get; set; }
        public string EnityName { get; set; }
        public string EnityGroup { get; set; }
        public string TablePrefix { get; set; }
    }
}
