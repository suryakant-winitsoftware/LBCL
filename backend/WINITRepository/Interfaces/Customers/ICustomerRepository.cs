using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
using static WINITRepository.Classes.Customers.PostgreSQLCustomerRepository;

namespace WINITRepository.Interfaces.Customers
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<WINITSharedObjects.Models.Customer>> SelectAllCustomers();
       // Task<WINITSharedObjects.Models.Customer> SelectCustomerByUID(string UID);
        Task<WINITSharedObjects.Models.Customer> SelectCustomerByUID(int Id);
        Task<WINITSharedObjects.Models.Customer> CreateCustomer(WINITSharedObjects.Models.Customer customer);
       // Task<WINITSharedObjects.Models.updateCustomer> UpdateCustomer(Int64 CustomerId, WINITSharedObjects.Models.updateCustomer updateCustomer);
      //  Task<int> UpdateCustomer(Int64 CustomerId, WINITSharedObjects.Models.updateCustomer updateCustomer);
        Task<int> UpdateCustomer(int Id, Customer Customer);
      //  Task<int> DeleteCustomer(Int64 CustomerId);
        Task<int> DeleteCustomer(int Id);
        Task<IEnumerable<WINITSharedObjects.Models.Customer>> GetCustomersFiltered(string Name, String Email);
        Task<IEnumerable<WINITSharedObjects.Models.Customer>> GetCustomersPaged(int pageNumber, int pageSize);
        //  Task<IEnumerable<WINITSharedObjects.Models.Customer>> GetCustomersSorted( string sortField,  SortDirection sortDirection);
        Task<IEnumerable<WINITSharedObjects.Models.Customer>> GetCustomersSorted(List<WINITSharedObjects.Enums.SortCriteria> sortCriterias);



    }
}
