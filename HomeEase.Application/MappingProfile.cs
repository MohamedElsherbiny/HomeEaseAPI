using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Application.Queries.PlatformService;
using HomeEase.Domain.Entities;

namespace HomeEase.Application.Mappings
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
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.StartingPrice, opt =>
                    opt.MapFrom(src => src.Services != null && src.Services.Any()
                        ? src.Services.Min(s => s.Price)
                        : (decimal?)null))
                  .ForMember(dest => dest.Gallery, opt =>
                  opt.MapFrom(src => src.Images
                  .Where(img => img.ImageType == ImageType.Gallery)
                  .OrderBy(img => img.SortOrder))) // Optional: order gallery images
                .ForMember(dest => dest.Logo, opt =>
                    opt.MapFrom(src => src.Images
                        .FirstOrDefault(img => img.ImageType == ImageType.Logo)))
                .ForMember(dest => dest.Cover, opt =>
                    opt.MapFrom(src => src.Images
                        .FirstOrDefault(img => img.ImageType == ImageType.Cover)));


            CreateMap<Provider, ProviderSearchResultDto>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Distance, opt => opt.Ignore())
                .ForMember(dest => dest.Gallery, opt =>
                  opt.MapFrom(src => src.Images
                  .Where(img => img.ImageType == ImageType.Gallery)
                  .OrderBy(img => img.SortOrder))) // Optional: order gallery images
                .ForMember(dest => dest.Logo, opt =>
                    opt.MapFrom(src => src.Images
                        .FirstOrDefault(img => img.ImageType == ImageType.Logo)))
                .ForMember(dest => dest.Cover, opt =>
                    opt.MapFrom(src => src.Images
                        .FirstOrDefault(img => img.ImageType == ImageType.Cover)));

            // Address mapping
            CreateMap<Address, AddressDto>();
            CreateMap<AddressDto, Address>();

            // Service mapping
            CreateMap<Service, ServiceDto>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.BasePlatformService.ImageUrl));

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
            CreateMap<UserServiceLike, UserServiceLikeDto>();

            CreateMap<User, UserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

            CreateMap<BasePlatformService, BasePlatformServiceDto>();

            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"));
            CreateMap<CreateReviewDto, Review>();
            CreateMap<UpdateReviewDto, Review>();


            CreateMap<ProviderImage, ProviderImageDto>();
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
