using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Application.DTOs.PlatformService;
using HomeEase.Application.DTOs.Provider;
using HomeEase.Application.DTOs.ProviderSchedule;
using HomeEase.Application.DTOs.ProviderService;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Enums;

namespace HomeEase.Application;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Provider mapping
        CreateMap<Provider, ProviderForUpdateDto>();
        CreateMap<Provider, ProviderDto>()
            .ForMember(dest => dest.StartingPrice, opt =>
                opt.MapFrom(src => src.Services != null && src.Services.Any()
                    ? src.Services.Min(s => s.Price)
                    : (decimal?)null))
            .ForMember(dest => dest.StartingHomePrice, opt =>
                opt.MapFrom(src => src.Services != null && src.Services.Any()
                    ? src.Services.Min(s => s.HomePrice)
                    : (decimal?)null))
              .ForMember(dest => dest.Gallery, opt =>
              opt.MapFrom(src => src.Images
              .Where(img => img.ImageType == ImageType.Gallery)
              .OrderBy(img => img.SortOrder)))
            .ForMember(dest => dest.Logo, opt =>
                opt.MapFrom(src => src.Images
                    .FirstOrDefault(img => img.ImageType == ImageType.Logo)))
            .ForMember(dest => dest.Cover, opt =>
                opt.MapFrom(src => src.Images
                    .FirstOrDefault(img => img.ImageType == ImageType.Cover)))
              .ForMember(dest => dest.BusinessName, opt => opt.MapFrom<LocalizedProviderBusinessNameResolver>())
            .ForMember(dest => dest.Description, opt => opt.MapFrom<LocalizedProviderDescriptionResolver>());


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
                    .FirstOrDefault(img => img.ImageType == ImageType.Cover)))
            .ForMember(dest => dest.BusinessName, opt => opt.MapFrom<LocalizedProviderBusinessNameResolver>());

        // Address mapping
        CreateMap<Address, AddressDto>();
        CreateMap<AddressDto, Address>();

        // Service mapping
        CreateMap<Service, ServiceDto>()
             .ForMember(dest => dest.Name, opt => opt.MapFrom<LocalizedServiceNameResolver>())
            .ForMember(dest => dest.Description, opt => opt.MapFrom<LocalizedServiceDescriptionResolver>())
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

    public class LocalizedProviderBusinessNameResolver(ICurrentUserService currentUserService) : IValueResolver<Provider, object, string>
    {
        public string Resolve(Provider source, object destination, string destMember, ResolutionContext context)
        {
            return currentUserService.Language == LanguageEnum.Ar ? source.BusinessNameAr ?? source.BusinessName : source.BusinessName;
        }
    }

    public class LocalizedProviderDescriptionResolver(ICurrentUserService currentUserService) : IValueResolver<Provider, object, string>
    {
        public string Resolve(Provider source, object destination, string destMember, ResolutionContext context)
        {
            return currentUserService.Language == LanguageEnum.Ar ? source.DescriptionAr ?? source.Description : source.Description;
        }
    }

    public class LocalizedServiceNameResolver(ICurrentUserService currentUserService) : IValueResolver<Service, object, string>
    {
        public string Resolve(Service source, object destination, string destMember, ResolutionContext context)
        {
            return currentUserService.Language == LanguageEnum.Ar
                ? source.NameAr ?? source.Name
                : source.Name;
        }
    }

    public class LocalizedServiceDescriptionResolver(ICurrentUserService currentUserService) : IValueResolver<Service, object, string>
    {
        public string Resolve(Service source, object destination, string destMember, ResolutionContext context)
        {
            return currentUserService.Language == LanguageEnum.Ar
                ? source.DescriptionAr ?? source.Description
                : source.Description;
        }
    }

}
