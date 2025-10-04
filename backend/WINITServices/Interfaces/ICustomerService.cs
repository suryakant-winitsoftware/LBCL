using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
//using static WINITRepository.Classes.Customers.PostgreSQLCustomerRepository;

namespace WINITServices.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<WINITSharedObjects.Models.Customer>> SelectAllCustomers();
        Task<WINITSharedObjects.Models.Customer> SelectCustomerByUID(int Id);
        Task<WINITSharedObjects.Models.Customer> CreateCustomer(Customer customer);
        Task<int> UpdateCustomer(int Id, Customer Customer);
        Task<int> DeleteCustomer(int Id);
        Task<IEnumerable<WINITSharedObjects.Models.Customer>> GetCustomersFiltered(string Name, String Email);
        Task<IEnumerable<WINITSharedObjects.Models.Customer>> GetCustomersPaged(int pageNumber, int pageSize);
        Task<IEnumerable<WINITSharedObjects.Models.Customer>> GetCustomersSorted(List<SortCriteria> sortCriterias);

    }
}
