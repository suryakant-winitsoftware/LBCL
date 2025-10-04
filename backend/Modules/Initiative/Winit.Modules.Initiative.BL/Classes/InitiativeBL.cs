using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Winit.Modules.Initiative.BL.Interfaces;
using Winit.Modules.Initiative.DL.Interfaces;
using Winit.Modules.Initiative.Model.Classes;
using Winit.Modules.Initiative.Model.Interfaces;
using Winit.Shared.Models.Common;

namespace Winit.Modules.Initiative.BL.Classes
{
    public class InitiativeBL : IInitiativeBL
    {
        private readonly IInitiativeDL _initiativeDL;
        private readonly IConfiguration _configuration;
        private readonly ILogger<InitiativeBL> _logger;
        private readonly IMapper _mapper;
        private readonly string _fileStoragePath;

        public InitiativeBL(
            IInitiativeDL initiativeDL,
            IConfiguration configuration,
            ILogger<InitiativeBL> logger,
            IMapper mapper)
        {
            _initiativeDL = initiativeDL;
            _configuration = configuration;
            _logger = logger;
            _mapper = mapper;
            _fileStoragePath = configuration["FileStorage:InitiativePath"] ?? "Data/Initiatives/UploadedFile";
        }

        #region Initiative Operations

        public async Task<InitiativeDTO> GetInitiativeByIdAsync(int initiativeId)
        {
            try
            {
                var initiative = await _initiativeDL.GetInitiativeByIdAsync(initiativeId);
                if (initiative == null)
                {
                    throw new KeyNotFoundException($"Initiative with ID {initiativeId} not found");
                }

                return MapToDTO(initiative);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting initiative by ID: {InitiativeId}", initiativeId);
                throw;
            }
        }

        public async Task<PagedResponse<InitiativeDTO>> GetInitiativesAsync(InitiativeSearchRequest searchRequest)
        {
            try
            {
                var initiatives = await _initiativeDL.GetAllInitiativesAsync(searchRequest);
                var totalCount = await _initiativeDL.GetInitiativeCountAsync(searchRequest);

                var dtos = initiatives.Select(MapToDTO).ToList();

                return new PagedResponse<InitiativeDTO>
                {
                    PagedData = dtos,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting initiatives");
                throw;
            }
        }

        public async Task<InitiativeDTO> CreateInitiativeAsync(CreateInitiativeRequest request, string userCode)
        {
            try
            {
                // Validate the request
                var validationResult = await ValidateInitiativeAsync(request);
                if (!validationResult.IsValid)
                {
                    throw new InvalidOperationException(string.Join(", ", validationResult.Errors));
                }

                // Create initiative object
                var initiative = new Model.Classes.Initiative
                {
                    AllocationNo = request.AllocationNo,
                    Name = request.Name,
                    Description = request.Description,
                    SalesOrgCode = request.SalesOrgCode,
                    Brand = request.Brand,
                    ContractAmount = request.ContractAmount,
                    ActivityType = request.ActivityType,
                    DisplayType = request.DisplayType,
                    DisplayLocation = request.DisplayLocation,
                    CustomerType = request.CustomerType,
                    CustomerGroup = request.CustomerGroup,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Status = "draft",
                    IsActive = true,
                    CreatedBy = userCode,
                    CreatedTime = DateTime.UtcNow
                };

                // Insert initiative
                var initiativeId = await _initiativeDL.InsertInitiativeAsync(initiative);
                initiative.InitiativeId = initiativeId;

                // Insert customers if provided
                if (request.CustomerCodes?.Any() == true)
                {
                    _logger.LogInformation($"Received {request.CustomerCodes.Count} customer codes: [{string.Join(", ", request.CustomerCodes)}]");

                    // Filter out null or empty customer codes
                    var validCustomerCodes = request.CustomerCodes.Where(code => !string.IsNullOrEmpty(code)).ToList();

                    if (!validCustomerCodes.Any())
                    {
                        _logger.LogWarning("All customer codes are null or empty, skipping customer insertion");
                    }
                    else
                    {
                        _logger.LogInformation($"Inserting {validCustomerCodes.Count} valid customers");

                        var customers = validCustomerCodes.Select(code => new InitiativeCustomer
                        {
                            InitiativeId = initiativeId,
                            CustomerCode = code,
                            DisplayType = request.DisplayType,
                            DisplayLocation = request.DisplayLocation,
                            ExecutionStatus = "pending"
                        }).Cast<IInitiativeCustomer>().ToList();

                        await _initiativeDL.InsertInitiativeCustomersAsync(initiativeId, customers);
                    }
                }

                // Insert products if provided
                if (request.Products?.Any() == true)
                {
                    var products = request.Products.Select(p => new InitiativeProduct
                    {
                        InitiativeId = initiativeId,
                        ItemCode = p.ItemCode,
                        ItemName = p.ItemName,
                        Barcode = p.Barcode,
                        PttPrice = p.PttPrice
                    }).Cast<IInitiativeProduct>().ToList();

                    await _initiativeDL.InsertInitiativeProductsAsync(initiativeId, products);
                }

                // Retrieve and return the complete initiative
                return await GetInitiativeByIdAsync(initiativeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating initiative");
                throw;
            }
        }

        public async Task<InitiativeDTO> UpdateInitiativeAsync(int initiativeId, CreateInitiativeRequest request, string userCode)
        {
            try
            {
                // Check if can edit
                if (!await CanEditInitiativeAsync(initiativeId, userCode))
                {
                    throw new InvalidOperationException("Initiative cannot be edited in its current state");
                }

                // Validate the request
                var validationResult = await ValidateInitiativeAsync(request, initiativeId);
                if (!validationResult.IsValid)
                {
                    throw new InvalidOperationException(string.Join(", ", validationResult.Errors));
                }

                // Get existing initiative
                var existingInitiative = await _initiativeDL.GetInitiativeByIdAsync(initiativeId);
                if (existingInitiative == null)
                {
                    throw new KeyNotFoundException($"Initiative with ID {initiativeId} not found");
                }

                // Update initiative properties
                existingInitiative.AllocationNo = request.AllocationNo;
                existingInitiative.Name = request.Name;
                existingInitiative.Description = request.Description;
                existingInitiative.SalesOrgCode = request.SalesOrgCode;
                existingInitiative.Brand = request.Brand;
                existingInitiative.ContractAmount = request.ContractAmount;
                existingInitiative.ActivityType = request.ActivityType;
                existingInitiative.DisplayType = request.DisplayType;
                existingInitiative.DisplayLocation = request.DisplayLocation;
                existingInitiative.CustomerType = request.CustomerType;
                existingInitiative.CustomerGroup = request.CustomerGroup;
                existingInitiative.StartDate = request.StartDate;
                existingInitiative.EndDate = request.EndDate;
                existingInitiative.ModifiedBy = userCode;
                existingInitiative.ModifiedTime = DateTime.UtcNow;

                // Update initiative
                await _initiativeDL.UpdateInitiativeAsync(existingInitiative);

                // Update customers
                if (request.CustomerCodes != null)
                {
                    var customers = request.CustomerCodes.Select(code => new InitiativeCustomer
                    {
                        InitiativeId = initiativeId,
                        CustomerCode = code,
                        DisplayType = request.DisplayType,
                        DisplayLocation = request.DisplayLocation,
                        ExecutionStatus = "pending"
                    }).Cast<IInitiativeCustomer>().ToList();

                    await _initiativeDL.InsertInitiativeCustomersAsync(initiativeId, customers);
                }

                // Update products
                if (request.Products != null)
                {
                    var products = request.Products.Select(p => new InitiativeProduct
                    {
                        InitiativeId = initiativeId,
                        ItemCode = p.ItemCode,
                        ItemName = p.ItemName,
                        Barcode = p.Barcode,
                        PttPrice = p.PttPrice
                    }).Cast<IInitiativeProduct>().ToList();

                    await _initiativeDL.InsertInitiativeProductsAsync(initiativeId, products);
                }

                // Retrieve and return the updated initiative
                return await GetInitiativeByIdAsync(initiativeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating initiative {InitiativeId}", initiativeId);
                throw;
            }
        }

        public async Task<bool> DeleteInitiativeAsync(int initiativeId, string userCode)
        {
            try
            {
                if (!await CanDeleteInitiativeAsync(initiativeId, userCode))
                {
                    throw new InvalidOperationException("Initiative cannot be deleted in its current state");
                }

                return await _initiativeDL.DeleteInitiativeAsync(initiativeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting initiative {InitiativeId}", initiativeId);
                throw;
            }
        }

        public async Task<InitiativeDTO> SubmitInitiativeAsync(int initiativeId, string userCode)
        {
            try
            {
                // Get initiative
                var initiative = await _initiativeDL.GetInitiativeByIdAsync(initiativeId);
                if (initiative == null)
                {
                    throw new KeyNotFoundException($"Initiative with ID {initiativeId} not found");
                }

                // Validate initiative has customers and products
                var customers = await _initiativeDL.GetInitiativeCustomersAsync(initiativeId);
                if (!customers.Any())
                {
                    throw new InvalidOperationException("Initiative must have at least one customer");
                }

                var products = await _initiativeDL.GetInitiativeProductsAsync(initiativeId);
                if (!products.Any())
                {
                    throw new InvalidOperationException("Initiative must have at least one product");
                }

                // Submit initiative
                var success = await _initiativeDL.SubmitInitiativeAsync(initiativeId, userCode);
                if (!success)
                {
                    throw new InvalidOperationException("Failed to submit initiative");
                }

                // Generate contract code if not exists
                if (string.IsNullOrEmpty(initiative.ContractCode))
                {
                    await _initiativeDL.GenerateContractCodeAsync(initiativeId);
                }

                return await GetInitiativeByIdAsync(initiativeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting initiative {InitiativeId}", initiativeId);
                throw;
            }
        }

        public async Task<bool> CancelInitiativeAsync(int initiativeId, string cancelReason, string userCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cancelReason))
                {
                    throw new ArgumentException("Cancel reason is required");
                }

                return await _initiativeDL.CancelInitiativeAsync(initiativeId, cancelReason, userCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling initiative {InitiativeId}", initiativeId);
                throw;
            }
        }

        public async Task<InitiativeDTO> SaveDraftAsync(int initiativeId, CreateInitiativeRequest request, string userCode)
        {
            try
            {
                if (initiativeId > 0)
                {
                    return await UpdateInitiativeAsync(initiativeId, request, userCode);
                }
                else
                {
                    return await CreateInitiativeAsync(request, userCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving draft for initiative {InitiativeId}", initiativeId);
                throw;
            }
        }

        #endregion

        #region Customer Operations

        public async Task<bool> UpdateInitiativeCustomersAsync(int initiativeId, List<InitiativeCustomerDTO> customers, string userCode)
        {
            try
            {
                if (!await CanEditInitiativeAsync(initiativeId, userCode))
                {
                    throw new InvalidOperationException("Initiative cannot be edited in its current state");
                }

                var customerEntities = customers.Select(c => new InitiativeCustomer
                {
                    InitiativeId = initiativeId,
                    CustomerCode = c.CustomerCode,
                    CustomerName = c.CustomerName,
                    DisplayType = c.DisplayType,
                    DisplayLocation = c.DisplayLocation,
                    ExecutionStatus = c.ExecutionStatus ?? "pending"
                }).Cast<IInitiativeCustomer>().ToList();

                return await _initiativeDL.InsertInitiativeCustomersAsync(initiativeId, customerEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating initiative customers for {InitiativeId}", initiativeId);
                throw;
            }
        }

        public async Task<List<InitiativeCustomerDTO>> GetInitiativeCustomersAsync(int initiativeId)
        {
            try
            {
                var customers = await _initiativeDL.GetInitiativeCustomersAsync(initiativeId);
                return customers.Select(c => new InitiativeCustomerDTO
                {
                    InitiativeCustomerId = c.InitiativeCustomerId,
                    CustomerCode = c.CustomerCode,
                    CustomerName = c.CustomerName,
                    DisplayType = c.DisplayType,
                    DisplayLocation = c.DisplayLocation,
                    ExecutionStatus = c.ExecutionStatus
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting initiative customers for {InitiativeId}", initiativeId);
                throw;
            }
        }

        #endregion

        #region Product Operations

        public async Task<bool> UpdateInitiativeProductsAsync(int initiativeId, List<InitiativeProductDTO> products, string userCode)
        {
            try
            {
                if (!await CanEditInitiativeAsync(initiativeId, userCode))
                {
                    throw new InvalidOperationException("Initiative cannot be edited in its current state");
                }

                var productEntities = products.Select(p => new InitiativeProduct
                {
                    InitiativeId = initiativeId,
                    ItemCode = p.ItemCode,
                    ItemName = p.ItemName,
                    Barcode = p.Barcode,
                    PttPrice = p.PttPrice
                }).Cast<IInitiativeProduct>().ToList();

                return await _initiativeDL.InsertInitiativeProductsAsync(initiativeId, productEntities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating initiative products for {InitiativeId}", initiativeId);
                throw;
            }
        }

        public async Task<List<InitiativeProductDTO>> GetInitiativeProductsAsync(int initiativeId)
        {
            try
            {
                var products = await _initiativeDL.GetInitiativeProductsAsync(initiativeId);
                return products.Select(p => new InitiativeProductDTO
                {
                    InitiativeProductId = p.InitiativeProductId,
                    ItemCode = p.ItemCode,
                    ItemName = p.ItemName,
                    Barcode = p.Barcode,
                    PttPrice = p.PttPrice
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting initiative products for {InitiativeId}", initiativeId);
                throw;
            }
        }

        #endregion

        #region Allocation Operations

        public async Task<List<AllocationMasterDTO>> GetAvailableAllocationsAsync(string salesOrgCode, string brand, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var allocations = await _initiativeDL.GetAllocationsAsync(salesOrgCode, brand, startDate, endDate);
                return allocations.Select(a => new AllocationMasterDTO
                {
                    AllocationNo = a.AllocationNo,
                    ActivityNo = a.ActivityNo,
                    AllocationName = a.AllocationName,
                    AllocationDescription = a.AllocationDescription,
                    TotalAllocationAmount = a.TotalAllocationAmount,
                    AvailableAllocationAmount = a.AvailableAllocationAmount,
                    ConsumedAmount = a.ConsumedAmount,
                    Brand = a.Brand,
                    SalesOrgCode = a.SalesOrgCode,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    DaysLeft = a.DaysLeft,
                    IsActive = a.IsActive
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available allocations");
                throw;
            }
        }

        public async Task<AllocationMasterDTO> GetAllocationDetailsAsync(string allocationNo)
        {
            try
            {
                var allocation = await _initiativeDL.GetAllocationByIdAsync(allocationNo);
                if (allocation == null)
                {
                    throw new KeyNotFoundException($"Allocation with number {allocationNo} not found");
                }

                return new AllocationMasterDTO
                {
                    AllocationNo = allocation.AllocationNo,
                    ActivityNo = allocation.ActivityNo,
                    AllocationName = allocation.AllocationName,
                    AllocationDescription = allocation.AllocationDescription,
                    TotalAllocationAmount = allocation.TotalAllocationAmount,
                    AvailableAllocationAmount = allocation.AvailableAllocationAmount,
                    ConsumedAmount = allocation.ConsumedAmount,
                    Brand = allocation.Brand,
                    SalesOrgCode = allocation.SalesOrgCode,
                    StartDate = allocation.StartDate,
                    EndDate = allocation.EndDate,
                    DaysLeft = allocation.DaysLeft,
                    IsActive = allocation.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting allocation details for {AllocationNo}", allocationNo);
                throw;
            }
        }

        public async Task<bool> ValidateAllocationAmountAsync(string allocationNo, decimal contractAmount, int? initiativeId = null)
        {
            try
            {
                return await _initiativeDL.ValidateContractAmountAsync(allocationNo, contractAmount, initiativeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating allocation amount");
                throw;
            }
        }

        #endregion

        #region File Operations

        public async Task<string> UploadFileAsync(int initiativeId, string fileType, byte[] fileContent, string fileName, string userCode)
        {
            try
            {
                if (!await CanEditInitiativeAsync(initiativeId, userCode))
                {
                    throw new InvalidOperationException("Initiative cannot be edited in its current state");
                }

                // Validate file type
                var allowedTypes = new[] { "posm", "default", "email" };
                if (!allowedTypes.Contains(fileType.ToLower()))
                {
                    throw new ArgumentException("Invalid file type");
                }

                // Validate file extension
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".xlsx", ".xls", ".mp4", ".mp3", ".mov", ".wav" };
                var extension = Path.GetExtension(fileName)?.ToLower();
                if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                {
                    throw new ArgumentException("Invalid file extension");
                }

                // Create file path
                var uniqueFileName = $"{DateTime.Now.Ticks}{extension}";
                var relativePath = Path.Combine(_fileStoragePath, uniqueFileName);
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);

                // Ensure directory exists
                var directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Save file
                await File.WriteAllBytesAsync(fullPath, fileContent);

                // Update initiative with file path
                var initiative = await _initiativeDL.GetInitiativeByIdAsync(initiativeId);
                if (initiative != null)
                {
                    switch (fileType.ToLower())
                    {
                        case "posm":
                            initiative.PosmFile = relativePath;
                            break;
                        case "default":
                            initiative.DefaultImage = relativePath;
                            break;
                        case "email":
                            initiative.EmailAttachment = relativePath;
                            break;
                    }

                    initiative.ModifiedBy = userCode;
                    initiative.ModifiedTime = DateTime.UtcNow;
                    await _initiativeDL.UpdateInitiativeAsync(initiative);
                }

                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file for initiative {InitiativeId}", initiativeId);
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(int initiativeId, string fileType, string userCode)
        {
            try
            {
                if (!await CanEditInitiativeAsync(initiativeId, userCode))
                {
                    throw new InvalidOperationException("Initiative cannot be edited in its current state");
                }

                var initiative = await _initiativeDL.GetInitiativeByIdAsync(initiativeId);
                if (initiative == null)
                {
                    throw new KeyNotFoundException($"Initiative with ID {initiativeId} not found");
                }

                string filePath = null;
                switch (fileType.ToLower())
                {
                    case "posm":
                        filePath = initiative.PosmFile;
                        initiative.PosmFile = null;
                        break;
                    case "default":
                        filePath = initiative.DefaultImage;
                        initiative.DefaultImage = null;
                        break;
                    case "email":
                        filePath = initiative.EmailAttachment;
                        initiative.EmailAttachment = null;
                        break;
                    default:
                        throw new ArgumentException("Invalid file type");
                }

                // Delete physical file if exists
                if (!string.IsNullOrEmpty(filePath))
                {
                    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                    }
                }

                // Update initiative
                initiative.ModifiedBy = userCode;
                initiative.ModifiedTime = DateTime.UtcNow;
                await _initiativeDL.UpdateInitiativeAsync(initiative);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file for initiative {InitiativeId}", initiativeId);
                throw;
            }
        }

        #endregion

        #region Validation Operations

        public async Task<ValidationResult> ValidateInitiativeAsync(CreateInitiativeRequest request, int? initiativeId = null)
        {
            var result = new ValidationResult { IsValid = true };

            try
            {
                // Required field validation
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    result.Errors.Add("Initiative name is required");
                }

                if (string.IsNullOrWhiteSpace(request.AllocationNo))
                {
                    result.Errors.Add("Allocation is required");
                }

                if (request.ContractAmount <= 0)
                {
                    result.Errors.Add("Contract amount must be greater than zero");
                }

                if (request.StartDate >= request.EndDate)
                {
                    result.Errors.Add("End date must be after start date");
                }

                // Check name uniqueness
                if (!string.IsNullOrWhiteSpace(request.Name))
                {
                    var isUnique = await _initiativeDL.IsInitiativeNameUniqueAsync(request.Name, initiativeId);
                    if (!isUnique)
                    {
                        result.Errors.Add("Initiative name already exists");
                    }
                }

                // Validate contract amount against allocation
                if (!string.IsNullOrWhiteSpace(request.AllocationNo) && request.ContractAmount > 0)
                {
                    var isValid = await _initiativeDL.ValidateContractAmountAsync(request.AllocationNo, request.ContractAmount, initiativeId);
                    if (!isValid)
                    {
                        result.Errors.Add("Contract amount exceeds available allocation amount");
                    }
                }

                result.IsValid = result.Errors.Count == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating initiative");
                result.IsValid = false;
                result.Errors.Add("An error occurred during validation");
            }

            return result;
        }

        public async Task<bool> CanEditInitiativeAsync(int initiativeId, string userCode)
        {
            try
            {
                return await _initiativeDL.CanEditInitiativeAsync(initiativeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if initiative {InitiativeId} can be edited", initiativeId);
                throw;
            }
        }

        public async Task<bool> CanDeleteInitiativeAsync(int initiativeId, string userCode)
        {
            try
            {
                return await _initiativeDL.CanEditInitiativeAsync(initiativeId); // Same as edit - only draft can be deleted
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if initiative {InitiativeId} can be deleted", initiativeId);
                throw;
            }
        }

        #endregion

        #region Private Helper Methods

        private InitiativeDTO MapToDTO(IInitiative initiative)
        {
            var dto = new InitiativeDTO
            {
                InitiativeId = initiative.InitiativeId,
                ContractCode = initiative.ContractCode,
                AllocationNo = initiative.AllocationNo,
                Name = initiative.Name,
                Description = initiative.Description,
                SalesOrgCode = initiative.SalesOrgCode,
                Brand = initiative.Brand,
                ContractAmount = initiative.ContractAmount,
                ActivityType = initiative.ActivityType,
                DisplayType = initiative.DisplayType,
                DisplayLocation = initiative.DisplayLocation,
                CustomerType = initiative.CustomerType,
                CustomerGroup = initiative.CustomerGroup,
                PosmFile = initiative.PosmFile,
                DefaultImage = initiative.DefaultImage,
                EmailAttachment = initiative.EmailAttachment,
                StartDate = initiative.StartDate,
                EndDate = initiative.EndDate,
                Status = initiative.Status,
                CancelReason = initiative.CancelReason,
                IsActive = initiative.IsActive
            };

            // Map customers if available
            if (initiative is Model.Classes.Initiative fullInitiative && fullInitiative.InitiativeCustomers != null)
            {
                dto.Customers = fullInitiative.InitiativeCustomers.Select(c => new InitiativeCustomerDTO
                {
                    InitiativeCustomerId = c.InitiativeCustomerId,
                    CustomerCode = c.CustomerCode,
                    CustomerName = c.CustomerName,
                    DisplayType = c.DisplayType,
                    DisplayLocation = c.DisplayLocation,
                    ExecutionStatus = c.ExecutionStatus
                }).ToList();
            }

            // Map products if available
            if (initiative is Model.Classes.Initiative fullInitiative2 && fullInitiative2.InitiativeProducts != null)
            {
                dto.Products = fullInitiative2.InitiativeProducts.Select(p => new InitiativeProductDTO
                {
                    InitiativeProductId = p.InitiativeProductId,
                    ItemCode = p.ItemCode,
                    ItemName = p.ItemName,
                    Barcode = p.Barcode,
                    PttPrice = p.PttPrice
                }).ToList();
            }

            return dto;
        }

        #endregion
    }
}