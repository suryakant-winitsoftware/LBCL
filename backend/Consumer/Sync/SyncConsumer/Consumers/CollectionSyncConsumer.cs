using MassTransit;
using SyncConsumer.Common.Collection.Interfaces;
using Winit.Modules.CollectionModule.BL.Interfaces;
using Winit.Modules.CollectionModule.Model.Classes;
using Winit.Modules.CollectionModule.Model.Interfaces;
using Winit.Modules.Syncing.Model.Interfaces;

namespace SyncConsumer.Consumers
{
    public class CollectionSyncConsumer : IConsumer<IAppRequest> 
    {
        private ICollectionModuleBL _collectionBL;
        private ICollectionMapper _collectionMapper;
        public CollectionSyncConsumer(ICollectionModuleBL collectionBL, ICollectionMapper collectionMapper)
        {
            _collectionBL = collectionBL;
            _collectionMapper = collectionMapper;
        }
        public async Task Consume(ConsumeContext<IAppRequest> context)
         {
            Console.WriteLine("Collection consumer processing...");
            int TrxStatus;
            string StatusMessage = string.Empty;
            List<ICollections> objCollectionList = new List<ICollections>();
            if (context == null)
            {
                return;
            }
            IAppRequest appRequest = context.Message;
            try
            {
                if (appRequest == null)
                {
                    return;
                }
                objCollectionList.Add(await _collectionMapper.MapDTOtoCollections(Newtonsoft.Json.JsonConvert.DeserializeObject<CollectionDTO>(appRequest.RequestBody)));
                if (objCollectionList == null)
                {
                    // Log.Information(ex, "Exception");
                    return;
                }

                StatusMessage = await _collectionBL.CreateReceipt(objCollectionList.ToArray());
                if (StatusMessage == "Successfully Inserted Data Into tables")
                {
                    TrxStatus = 1;
                }
                else
                {
                    TrxStatus = 0;
                    throw new Exception();
                }
                Console.WriteLine("Collection consumer processing completed");
            }
            catch (Exception ex)
            {
                // Log.Error(ex, "Exception");
                throw;
            }
        }
        
    }
}
