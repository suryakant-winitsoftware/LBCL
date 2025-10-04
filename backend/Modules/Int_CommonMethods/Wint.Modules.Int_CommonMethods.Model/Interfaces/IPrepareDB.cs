using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.Model;

namespace Winit.Modules.Int_CommonMethods.Model.Interfaces
{
    public interface IPrepareDB : IBaseModel
    {         
        public string Script { get; set; }
        public string EnityName { get; set; }
        public string EnityGroup { get; set; }
        public string TablePrefix { get; set; }

    }
}
