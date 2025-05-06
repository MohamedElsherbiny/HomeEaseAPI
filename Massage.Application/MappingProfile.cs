using AutoMapper;
using Massage.Application.DTOs;
using Massage.Domain.Entities;

namespace Massage.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Provider mapping
            CreateMap<Provider, ProviderDto>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Schedule, opt => opt.MapFrom(src => src.Schedule))
                .ForMember(dest => dest.Services, opt => opt.MapFrom(src => src.Services));

            CreateMap<Provider, ProviderSearchResultDto>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Distance, opt => opt.Ignore());

            // Address mapping
            CreateMap<Address, AddressDto>();
            CreateMap<AddressDto, Address>();

            // Service mapping
            CreateMap<Service, ServiceDto>();
            CreateMap<CreateServiceDto, Service>();
            CreateMap<UpdateServiceDto, Service>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Schedule mapping
            CreateMap<ProviderSchedule, ProviderScheduleDto>();
            CreateMap<ProviderScheduleDto, ProviderSchedule>();

            CreateMap<WorkingHours, WorkingHoursDto>();
            CreateMap<WorkingHoursDto, WorkingHours>();

            CreateMap<SpecialDate, SpecialDateDto>();
            CreateMap<SpecialDateDto, SpecialDate>();

            CreateMap<TimeSlot, TimeSlotDto>();
            CreateMap<TimeSlotDto, TimeSlot>();
        }
    }

    public class BookingMappingProfile : Profile
    {
        public BookingMappingProfile()
        {
            CreateMap<Booking, BookingDto>()
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"))
                .ForMember(dest => dest.ProviderBusinessName, opt => opt.MapFrom(src => src.Provider.BusinessName))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service.Name));

            CreateMap<PaymentInfo, PaymentInfoDto>();
            CreateMap<PaymentInfoDto, PaymentInfo>();
        }
    }
}
