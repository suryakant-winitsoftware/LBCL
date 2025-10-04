using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CollectionModule.Model.Interfaces;

namespace Winit.Modules.CollectionModule.Model.Classes
{
    public class AccCollectionAndDeposit : IAccCollectionAndDeposit
    {
        public List<IAccCollectionDeposit> accCollectionDeposits { get; set; }
        public List<IAccCollection> accCollections { get; set; }
    }
}
