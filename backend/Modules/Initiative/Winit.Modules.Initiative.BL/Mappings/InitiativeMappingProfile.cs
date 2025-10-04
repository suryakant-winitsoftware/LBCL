using AutoMapper;
using Winit.Modules.Initiative.BL.Interfaces;
using Winit.Modules.Initiative.Model.Classes;
using Winit.Modules.Initiative.Model.Interfaces;

namespace Winit.Modules.Initiative.BL.Mappings
{
    public class InitiativeMappingProfile : Profile
    {
        public InitiativeMappingProfile()
        {
            CreateMap<IInitiative, InitiativeDTO>()
                .ForMember(dest => dest.InitiativeId, opt => opt.MapFrom(src => src.InitiativeId))
                .ForMember(dest => dest.ContractCode, opt => opt.MapFrom(src => src.ContractCode))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.AllocationNo, opt => opt.MapFrom(src => src.AllocationNo))
                .ForMember(dest => dest.ContractAmount, opt => opt.MapFrom(src => src.ContractAmount))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.ActivityType, opt => opt.MapFrom(src => src.ActivityType))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.CancelReason, opt => opt.MapFrom(src => src.CancelReason))
                .ForMember(dest => dest.SalesOrgCode, opt => opt.MapFrom(src => src.SalesOrgCode))
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ReverseMap();

            CreateMap<Model.Classes.Initiative, InitiativeDTO>().ReverseMap();
            CreateMap<Model.Classes.Initiative, IInitiative>().ReverseMap();

            CreateMap<IInitiativeCustomer, InitiativeCustomerDTO>()
                .ForMember(dest => dest.CustomerCode, opt => opt.MapFrom(src => src.CustomerCode))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.CustomerName))
                .ReverseMap();

            CreateMap<InitiativeCustomer, InitiativeCustomerDTO>().ReverseMap();
            CreateMap<InitiativeCustomer, IInitiativeCustomer>().ReverseMap();

            CreateMap<IInitiativeProduct, InitiativeProductDTO>()
                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.ItemCode))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.ItemName))
                .ReverseMap();

            CreateMap<InitiativeProduct, InitiativeProductDTO>().ReverseMap();
            CreateMap<InitiativeProduct, IInitiativeProduct>().ReverseMap();

            CreateMap<IAllocationMaster, AllocationMasterDTO>()
                .ForMember(dest => dest.AllocationNo, opt => opt.MapFrom(src => src.AllocationNo))
                .ForMember(dest => dest.ActivityNo, opt => opt.MapFrom(src => src.ActivityNo))
                .ForMember(dest => dest.AllocationName, opt => opt.MapFrom(src => src.AllocationName))
                .ForMember(dest => dest.AllocationDescription, opt => opt.MapFrom(src => src.AllocationDescription))
                .ForMember(dest => dest.TotalAllocationAmount, opt => opt.MapFrom(src => src.TotalAllocationAmount))
                .ForMember(dest => dest.AvailableAllocationAmount, opt => opt.MapFrom(src => src.AvailableAllocationAmount))
                .ForMember(dest => dest.ConsumedAmount, opt => opt.MapFrom(src => src.ConsumedAmount))
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand))
                .ForMember(dest => dest.SalesOrgCode, opt => opt.MapFrom(src => src.SalesOrgCode))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.DaysLeft, opt => opt.MapFrom(src => (int?)(src.EndDate - DateTime.Today).Days))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ReverseMap();

            CreateMap<AllocationMaster, AllocationMasterDTO>().ReverseMap();
            CreateMap<AllocationMaster, IAllocationMaster>().ReverseMap();

            CreateMap<CreateInitiativeRequest, Model.Classes.Initiative>()
                .ForMember(dest => dest.InitiativeId, opt => opt.Ignore())
                .ForMember(dest => dest.ContractCode, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore());
        }
    }
}