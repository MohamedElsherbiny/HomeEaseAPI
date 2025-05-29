using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Application.Queries.PlatformService;
using HomeEase.Domain.Entities;

namespace HomeEase.Application
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Provider mapping
            CreateMap<Provider, ProviderDto>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Schedule, opt => opt.MapFrom(src => src.Schedule))
                .ForMember(dest => dest.Services, opt => opt.MapFrom(src => src.Services))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));

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


            CreateMap<User, UserDto>();
            CreateMap<UserPreferences, UserPreferencesDto>();
            CreateMap<UserPreferencesDto, UserPreferences>();

            CreateMap<User, UserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

            CreateMap<BasePlatformService, BasePlatformServiceDto>();

            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"));
            CreateMap<CreateReviewDto, Review>();
            CreateMap<UpdateReviewDto, Review>();
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
            CreateMap<CreatePaymentDto, PaymentInfo>();
            CreateMap<UpdatePaymentDto, PaymentInfo>();
            CreateMap<PaymentResult, PaymentResultDto>();
        }
    }
}
