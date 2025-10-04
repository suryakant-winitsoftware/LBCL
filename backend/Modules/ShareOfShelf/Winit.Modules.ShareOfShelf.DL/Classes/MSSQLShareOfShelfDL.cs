using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.ShareOfShelf.DL.Interfaces;
using Winit.Modules.ShareOfShelf.Model.Interfaces;

namespace Winit.Modules.ShareOfShelf.DL.Classes
{
    public class MSSQLShareOfShelfDL : Winit.Modules.Base.DL.DBManager.SqlServerDBManager, IShareOfShelfDL
    {
        private readonly ILogger<MSSQLShareOfShelfDL> _logger;
        public MSSQLShareOfShelfDL(IServiceProvider serviceProvider, IConfiguration config, ILogger<MSSQLShareOfShelfDL> logger) : base(serviceProvider, config)
        {
            _logger = logger;
        }

        public Task<IEnumerable<ISosHeaderCategoryItemView>> GetCategories(string SosHeaderUID)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ISosLine>> GetShelfDataByCategoryUID(string CategoryUID)
        {
            throw new NotImplementedException();
        }

        public Task<ISosHeader> GetSosHeaderDetails(string StoreUID)
        {
            throw new NotImplementedException();
        }

        public Task<int> SaveShelfData(IEnumerable<ISosLine> ShelfData)
        {
            throw new NotImplementedException();
        }
    }
}
