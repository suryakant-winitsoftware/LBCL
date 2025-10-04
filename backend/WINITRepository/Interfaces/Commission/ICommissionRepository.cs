using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
using static WINITRepository.Classes.Commission.PostgreSQLCommissionRepository;

namespace WINITRepository.Interfaces.Commission
{
    public interface ICommissionRepository
    {
        Task<int> ProcessCommission();
    }
}
