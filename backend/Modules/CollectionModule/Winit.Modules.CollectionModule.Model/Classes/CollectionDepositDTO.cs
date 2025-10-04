using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CollectionModule.Model.Interfaces;

namespace Winit.Modules.CollectionModule.Model.Classes
{
    public class CollectionDepositDTO
    {
        public List<AccCollectionDeposit> AccCollectionDeposits { get; set; }
        public List<AccCollection> AccCollections { get; set; }
    }
}
