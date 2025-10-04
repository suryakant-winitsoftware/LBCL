using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
using static WINITRepository.Classes.Commission.PostgreSQLCommissionRepository;

namespace WINITServices.Classes.Commission
{
    public class CommissionService : CommissionBaseService
    {
        public CommissionService(WINITRepository.Interfaces.Commission.ICommissionRepository commissionRepository) : base(commissionRepository)
        {

        }
        public async override Task<int> ProcessCommission()
        {
            return await _commissionRepository.ProcessCommission();
        }



    }
}
