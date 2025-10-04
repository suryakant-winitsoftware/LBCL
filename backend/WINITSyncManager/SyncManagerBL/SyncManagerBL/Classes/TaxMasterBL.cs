using SyncManagerBL.Interfaces;
using SyncManagerDL.Interfaces;
using SyncManagerModel.Interfaces;

namespace SyncManagerBL.Classes
{
    public class TaxMasterBL : ITaxMasterBL
    {
        private readonly ITaxMasterDL _taxMaster;
        private readonly ITaxMasterStagingDL _taxMasterStaging;
        public TaxMasterBL(ITaxMasterDL taxMasterDL,ITaxMasterStagingDL taxMasterStaging)
        {
            _taxMaster = taxMasterDL;
            _taxMasterStaging = taxMasterStaging;
        }
        public async Task<List<ITaxMaster>> GetTaxMasterDetails(string sql)
        {
            return await _taxMaster.GetTaxMasterDetails(sql);
        }

        public async Task<int> InsertTaxDataIntoMonthTable(List<ITaxMaster> taxMasters, IEntityDetails entityDetails)
        {
             return await _taxMasterStaging.InsertTaxDataIntoMonthTable(taxMasters, entityDetails); 
        }

    }
}
