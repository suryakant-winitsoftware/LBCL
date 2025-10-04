using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using WINITRepository.Interfaces.Customers;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;

namespace WINITRepository.Classes.Customers
{
    public class PostgreSQLCustomerRepository : Interfaces.Customers.ICustomerRepository
    {
        private readonly string _connectionString;
        public PostgreSQLCustomerRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("PostgreSQL");
        }
        public async Task<IEnumerable<WINITSharedObjects.Models.Customer>> SelectAllCustomers()
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.Customer> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.Customer>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
            };

            // var sql = "SELECT * FROM Customer";
            var sql = "SELECT * FROM Customers";

            IEnumerable<WINITSharedObjects.Models.Customer> customerList = await dbManager.ExecuteQueryAsync(sql, parameters);
            return await Task.FromResult(customerList);
        }
        public async Task<WINITSharedObjects.Models.Customer> SelectCustomerByUID(int Id)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.Customer> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.Customer>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"Id",  Id}
            };

            //  var sql = @"SELECT * FROM Customer WHERE ""UID"" = @UID";
            var sql = @"SELECT * FROM Customers WHERE ""Id"" = @Id";

            WINITSharedObjects.Models.Customer customer = await dbManager.ExecuteSingleAsync(sql, parameters);
            return await Task.FromResult(customer);
        }

        public async Task<WINITSharedObjects.Models.Customer> CreateCustomer(WINITSharedObjects.Models.Customer customer)
        {
            DBManager.PostgresDBManager<Customer> dbManager = new DBManager.PostgresDBManager<Customer>(_connectionString);
            //  var sql = @"INSERT INTO customer (""UID"", ""CustomerCode"", ""CustomerName"") VALUES (@UID, @CustomerCode, @CustomerName)";
            var sql = @"INSERT INTO customers (""Name"", ""ContactNumber"", ""TerritoryId"",""Email"",""CreatedDate"") VALUES (@Name, @ContactNumber, @TerritoryId,@Email,@CreatedDate)";
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {

                   {"Name", customer.Name},
                   {"ContactNumber", customer.ContactNumber},
                   {"TerritoryId", customer.TerritoryId},
                   {"Email", customer.Email},
                   {"CreatedDate", customer.CreatedDate},

             
             };
            await dbManager.ExecuteNonQueryAsync(sql, parameters);
            return customer;
        }

        public async Task<int> UpdateCustomer(int Id, Customer Customer)
        {
            try
            {
                DBManager.PostgresDBManager<Customer> dbManager = new DBManager.PostgresDBManager<Customer>(_connectionString);
                var sql = @"UPDATE customers SET ""Name""=@Name,""ContactNumber""=@ContactNumber,""TerritoryId""=@TerritoryId,""LastModifiedDate""=@LastModifiedDate WHERE ""Id""=@Id";
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                        {"Name", Customer.Name},
                        {"ContactNumber", Customer.ContactNumber},
                        {"TerritoryId", Customer.TerritoryId},
                        {"Email", Customer.Email},
                        {"LastModifiedDate", Customer.LastModifiedDate},
                        {"Id", Id},
                 };
                var customer = await dbManager.ExecuteNonQueryAsync(sql, parameters);
                return customer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            await Task.Delay(1000);
            return 1;
        }
        public async Task<int> DeleteCustomer(int Id)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.Customer> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.Customer>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"Id",  Id}
            };
            var sql = @"DELETE  FROM Customers WHERE ""Id"" = @Id";

            var customer = await dbManager.ExecuteNonQueryAsync(sql, parameters);
            return customer;
        }
        public async Task<IEnumerable<WINITSharedObjects.Models.Customer>> GetCustomersFiltered(string Name, String Email)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.Customer> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.Customer>(_connectionString);

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(Name))
            {
                parameters.Add("Name", Name);
            }
            else
            {
                parameters.Add("Name", DBNull.Value);
            }
            if (!string.IsNullOrEmpty(Email))
            {
                parameters.Add("Email", Email);
            }
            else
            {
                parameters.Add("Email", DBNull.Value);
            }

            //            var sql = @"SELECT * FROM Customer WHERE (""UID"" LIKE '%' || @UID || '%' OR @UID IS NULL) AND (""CustomerCode"" LIKE '%' || @CustomerCode || '%' OR @CustomerCode IS NULL) 
            //AND (""CustomerName"" LIKE '%' || @CustomerName || '%' OR @CustomerName IS NULL)";
            var sql = @"SELECT * FROM Customers WHERE (""Name"" LIKE '%' || @Name || '%' OR ""Email"" LIKE '%' || @Email || '%')";

            IEnumerable<WINITSharedObjects.Models.Customer> customerList = await dbManager.ExecuteQueryAsync(sql, parameters);
            return await Task.FromResult(customerList);
        }
        public async Task<IEnumerable<WINITSharedObjects.Models.Customer>> GetCustomersPaged(int pageNumber, int pageSize)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.Customer> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.Customer>(_connectionString);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            var sql = @"SELECT * FROM Customers OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";
            parameters.Add("Offset", (pageNumber - 1) * pageSize);
            parameters.Add("Limit", pageSize);
            IEnumerable<WINITSharedObjects.Models.Customer> customerList = await dbManager.ExecuteQueryAsync(sql, parameters);
            return await Task.FromResult(customerList);
        }

        public async Task<IEnumerable<WINITSharedObjects.Models.Customer>> GetCustomersSorted(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias)
        {
            DBManager.PostgresDBManager<WINITSharedObjects.Models.Customer> dbManager = new DBManager.PostgresDBManager<WINITSharedObjects.Models.Customer>(_connectionString);

            var sql = new StringBuilder("SELECT * FROM Customers ORDER BY ");
            var sortParameters = new Dictionary<string, object>();
            if (sortCriterias != null && sortCriterias.Count > 0)
            {
                for (var i = 0; i < sortCriterias.Count; i++)
                {
                    var paramName = $"{i}";
                    // var paramName = $"{sortCriterias[i].SortParameter}";

                    sql.Append($"@{paramName} {(sortCriterias[i].Direction == SortDirection.Asc ? "ASC" : "DESC")}");

                    if (i < sortCriterias.Count - 1)
                    {
                        sql.Append(", ");
                    }

                    sortParameters.Add(paramName, sortCriterias[i].SortParameter);
                }
            }

            IEnumerable<WINITSharedObjects.Models.Customer> customerList = await dbManager.ExecuteQueryAsync(sql.ToString(), sortParameters);

            return customerList;
        }

    }
}








