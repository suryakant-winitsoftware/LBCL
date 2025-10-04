using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WINITSharedObjects.Models;
using WINITSharedObjects.Enums;

namespace WINITRepository.Classes.Customers
{
    public class SqliteCustomersRepository : Interfaces.Customers.ICustomerRepository
    {
        private readonly string _connectionString;
        public SqliteCustomersRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("SQLite");
        }

       

        public async Task<IEnumerable<WINITSharedObjects.Models.Customer>> SelectAllCustomers()
        {
            IEnumerable < WINITSharedObjects.Models.Customer > customerList = new List<WINITSharedObjects.Models.Customer>() {
                //new WINITSharedObjects.Models.Customer{CustomerId = 1, UID = "1", CustomerCode ="C0001", CustomerName = "Customer1" },
                //new WINITSharedObjects.Models.Customer{CustomerId = 2, UID = "2", CustomerCode ="C0002", CustomerName = "Customer2" },
                //new WINITSharedObjects.Models.Customer{CustomerId = 3, UID = "3", CustomerCode ="C0003", CustomerName = "Customer3" }
                    };
            return await Task.FromResult(customerList);
        }
        public async Task<WINITSharedObjects.Models.Customer> SelectCustomerByUID(int Id)
        {
            WINITSharedObjects.Models.Customer customer = new WINITSharedObjects.Models.Customer(); 
           //{ CustomerId = 1, UID = "1", CustomerCode = "C0001", CustomerName = "Customer1" };
            return await Task.FromResult(customer);
        }
        public Task<Customer> CreateCustomer(Customer customer)
        {
            throw new NotImplementedException();
        }
        //public Task<int> UpdateCustomer(Int64 CustomerId, updateCustomer updateCustomer)
        //{
        //    throw new NotImplementedException();
        //}
        public Task<int> UpdateCustomer(int Id, Customer Customer)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteCustomer(int Id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Customer>> GetCustomersFiltered(string Name, String Email)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Customer>> GetCustomersPaged(int pageNumber, int pageSize)
        {
            throw new NotImplementedException();
        }

        //public Task<IEnumerable<Customer>> GetCustomersSorted(string sortField, SortDirection sortDirection)
        //{
        //    throw new NotImplementedException();
        //}

        public Task<IEnumerable<Customer>> GetCustomersSorted(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias)
        {
            throw new NotImplementedException();
        }

       
    }
}
