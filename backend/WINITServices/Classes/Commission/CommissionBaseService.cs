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
    public abstract class CommissionBaseService : Interfaces.ICommissionService
    {
        protected readonly WINITRepository.Interfaces.Commission.ICommissionRepository _commissionRepository;
        public CommissionBaseService(WINITRepository.Interfaces.Commission.ICommissionRepository commissionRepository)
        {
            _commissionRepository = commissionRepository;
        }
        public abstract Task<int> ProcessCommission();
        
    }
}
