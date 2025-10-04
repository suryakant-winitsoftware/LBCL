using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WINITSharedObjects.Constants;
using WINITSharedObjects.Enums;
using WINITSharedObjects.Models;
using static WINITRepository.Classes.Customers.PostgreSQLCustomerRepository;

namespace WINITServices.Classes.Customer
{
    public abstract class CustomerBaseService : Interfaces.ICustomerService
    {
        protected readonly WINITRepository.Interfaces.Customers.ICustomerRepository _customerRepository;
        public CustomerBaseService(WINITRepository.Interfaces.Customers.ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }
        public abstract Task<int> AddCustomer();
        //public abstract Task<int> UpdateCustomer();
        //public abstract Task<int> DeleteCustomer();
        public abstract Task<IEnumerable<WINITSharedObjects.Models.Customer>> SelectAllCustomers();
     //   public abstract Task<WINITSharedObjects.Models.Customer> SelectCustomerByUID(string UID);
        public abstract Task<WINITSharedObjects.Models.Customer> SelectCustomerByUID(int Id);

        public abstract Task<WINITSharedObjects.Models.Customer> CreateCustomer(WINITSharedObjects.Models.Customer customer);
      //  public abstract Task<WINITSharedObjects.Models.updateCustomer> UpdateCustomer(Int64 CustomerId,WINITSharedObjects.Models.updateCustomer updateCustomer);
        //public abstract Task<int> UpdateCustomer(Int64 CustomerId,WINITSharedObjects.Models.updateCustomer updateCustomer);
        public abstract Task<int> UpdateCustomer( int Id,WINITSharedObjects.Models.Customer Customer);
      //  public abstract Task<int> DeleteCustomer(Int64 CustomerId);
        public abstract Task<int> DeleteCustomer(int Id);
        public abstract Task<IEnumerable<WINITSharedObjects.Models.Customer>> GetCustomersFiltered(string Name, String Email);
        public abstract Task<IEnumerable<WINITSharedObjects.Models.Customer>> GetCustomersPaged(int pageNumber, int pageSize);
        //public abstract Task<IEnumerable<WINITSharedObjects.Models.Customer>> GetCustomersSorted(string sortField, SortDirection sortDirection);
        public abstract Task<IEnumerable<WINITSharedObjects.Models.Customer>> GetCustomersSorted(List<SortCriteria> sortCriterias);




    }
}
