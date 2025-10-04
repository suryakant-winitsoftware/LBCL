using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winit.Modules.CollectionModule.Model.Interfaces
{
    public interface IAccCollectionAndDeposit
    {
        public List<IAccCollectionDeposit> accCollectionDeposits { get; set; }
        public List<IAccCollection> accCollections { get; set; }
    }
}
