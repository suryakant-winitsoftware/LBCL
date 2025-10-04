using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winit.Modules.Base.DL.DBManager;

namespace Winit.Modules.Base.BL
{
    public class MobileBase : SqliteDBManager
    {
        public MobileBase(IServiceProvider serviceProvider, IConfiguration config) : base(serviceProvider)
        {

        }
        public async Task<List<T>> GetList<T>(string sql, IDictionary<string, object?>? parameters = null)
        {
            return await ExecuteQueryAsync<T>(sql, parameters);
        }

        public async Task<int> Insert(string query, IDictionary<string, object?>? parameters = null)
        {
            return await ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<int> ExecuteScaler(string query, IDictionary<string, object?>? parameters = null)
        {
            return await ExecuteScalarAsync<int>(query, parameters);
        }

    }
}
