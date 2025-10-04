using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WINITSharedObjects.Models;
using WINITSharedObjects.Enums;

namespace WINITRepository.Classes.Commission
{
    public class SqliteCommissionRepository : Interfaces.Commission.ICommissionRepository
    {
        private readonly string _connectionString;
        public SqliteCommissionRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("SQLite");
        }

        public async Task<int> ProcessCommission()
        {
            int isprocess = 0;
            IEnumerable < WINITSharedObjects.Models.Customer > customerList = new List<WINITSharedObjects.Models.Customer>() {
                //new WINITSharedObjects.Models.Customer{CustomerId = 1, UID = "1", CustomerCode ="C0001", CustomerName = "Customer1" },
                //new WINITSharedObjects.Models.Customer{CustomerId = 2, UID = "2", CustomerCode ="C0002", CustomerName = "Customer2" },
                //new WINITSharedObjects.Models.Customer{CustomerId = 3, UID = "3", CustomerCode ="C0003", CustomerName = "Customer3" }
                    };
            return await Task.FromResult(isprocess);
        }
       
    }
}
