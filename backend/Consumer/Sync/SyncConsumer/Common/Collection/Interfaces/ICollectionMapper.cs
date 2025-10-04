using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CollectionModule.Model.Classes;

namespace SyncConsumer.Common.Collection.Interfaces
{
    public interface ICollectionMapper
    {
        Task<Collections> MapDTOtoCollections(CollectionDTO collectionDTO);
    }
}
