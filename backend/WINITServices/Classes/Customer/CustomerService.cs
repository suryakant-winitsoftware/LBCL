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
    public class CustomerService : CustomerBaseService
    {
        public CustomerService(WINITRepository.Interfaces.Customers.ICustomerRepository customerRepository) : base(customerRepository)
        {

        }
        public async override Task<int> AddCustomer()
        {
            throw new NotImplementedException();
        }

        //public async override Task<int> DeleteCustomer()
        //{
        //    throw new NotImplementedException();
        //}

        public async override Task<IEnumerable<WINITSharedObjects.Models.Customer>> SelectAllCustomers()
        {
            return await _customerRepository.SelectAllCustomers();
        }

        public async override Task<WINITSharedObjects.Models.Customer> SelectCustomerByUID(int Id)
        {
            return await _customerRepository.SelectCustomerByUID(Id);
        }
        public async override Task<WINITSharedObjects.Models.Customer> CreateCustomer(WINITSharedObjects.Models.Customer customer)
        {
            return await _customerRepository.CreateCustomer(customer);

        }
        //public async override Task<WINITSharedObjects.Models.updateCustomer> UpdateCustomer(Int64 CustomerId, WINITSharedObjects.Models.updateCustomer updateCustomer)
        //{
        //    return await _customerRepository.UpdateCustomer(CustomerId, updateCustomer);
        //}
        //public async override Task<int> UpdateCustomer(Int64 CustomerId, WINITSharedObjects.Models.updateCustomer updateCustomer)
        //{
        //    return await _customerRepository.UpdateCustomer(CustomerId, updateCustomer);
        //}

        //public async override Task<int> DeleteCustomer(Int64 CustomerId)
        //{
        //    return await _customerRepository.DeleteCustomer(CustomerId);
        //}
        public async override Task<int> UpdateCustomer(int Id, WINITSharedObjects.Models.Customer Customer)
        {
            return await _customerRepository.UpdateCustomer(Id, Customer);
        }
        public async override Task<int> DeleteCustomer(int Id)
        {
            return await _customerRepository.DeleteCustomer(Id);
        }
        public async override Task<IEnumerable<WINITSharedObjects.Models.Customer>> GetCustomersFiltered(string Name, String Email)
        {
            return await _customerRepository.GetCustomersFiltered(Name, Email);
        }

        public async override Task<IEnumerable<WINITSharedObjects.Models.Customer>> GetCustomersPaged(int pageNumber, int pageSize)
        {
            return await _customerRepository.GetCustomersPaged(pageNumber,pageSize);
        }

        //public async override Task<IEnumerable<WINITSharedObjects.Models.Customer>> GetCustomersSorted(string sortField, SortDirection sortDirection)
        //{
        //    return await _customerRepository.GetCustomersSorted(sortField, sortDirection);
        //}
        public async override Task<IEnumerable<WINITSharedObjects.Models.Customer>> GetCustomersSorted(List<SortCriteria> sortCriterias)
        {
            return await _customerRepository.GetCustomersSorted(sortCriterias);
        }



    }
}
