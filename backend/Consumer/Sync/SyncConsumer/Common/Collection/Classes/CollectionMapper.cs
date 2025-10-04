using SyncConsumer.Common.Collection.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;

namespace SyncConsumer.Common.Collection.Classes
{
    public class CollectionMapper : ICollectionMapper
    {
        public async Task<Collections> MapDTOtoCollections(CollectionDTO collectionDTO)
        {
            try
            {
                Collections collections = new Collections();
                collections.AccCollection = collectionDTO.AccCollection;
                collections.AccCollectionPaymentMode = collectionDTO.AccCollectionPaymentMode;
                collections.AccStoreLedger = collectionDTO.AccStoreLedger;
                collections.AccCollectionAllotment = collectionDTO.AccCollectionAllotment.ToList<IAccCollectionAllotment>();
                collections.AccPayable = collectionDTO.AccPayable?.ToList<IAccPayable>() ?? new List<IAccPayable>();
                collections.AccReceivable = collectionDTO.AccReceivable?.ToList<IAccReceivable>() ?? new List<IAccReceivable>();
                collections.AccCollectionCurrencyDetails = collectionDTO.AccCollectionCurrencyDetails?.ToList<IAccCollectionCurrencyDetails>() ?? new List<IAccCollectionCurrencyDetails>();
                return collections;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
