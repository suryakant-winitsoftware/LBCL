using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Int_CommonMethods.BL.Interfaces;
using Winit.Modules.Int_CommonMethods.Model.Interfaces;
using WINITSyncDataProcessor.Constants;

namespace WINITSyncDataProcessor.Interfacess
{
    public class ItemMasterSync
    {
        private readonly Iint_CommonMethodsBL _int_CommmonMethods;
        public ItemMasterSync(Iint_CommonMethodsBL int_CommmonMethods)
        {
            _int_CommmonMethods = int_CommmonMethods;
        }

        public async Task<int> ItemMasterProcess()
        {
            try
            {
                int currentRunningProcess;long  sycLogId;
                //Prepare DataBase
                await _int_CommmonMethods.PrepareDBByEntity(EntityNames.ItemMaster);
                //Checking Current Running Process
                  currentRunningProcess = await _int_CommmonMethods.CheckCurrentRunningProcess(EntityNames.ItemMaster);
                if (currentRunningProcess == 0)
                {
                    //Initiate Process
                    sycLogId = await _int_CommmonMethods.InitiateProcess(EntityNames.ItemMaster);
                    if (sycLogId > 0)
                    {
                        // Fetch Entity To Sync
                        IEntityDetails entityDetails = await _int_CommmonMethods.FetchEntityDetails(EntityNames.ItemMaster, sycLogId);
                        if (entityDetails != null)
                        {

                        }

                    }

                } 
            }
            catch
            {
                throw;

            }

            return 0;


        }
    }
}
