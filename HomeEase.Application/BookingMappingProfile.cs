using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Application.DTOs.Booking;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Entities;
using HomeEase.Domain.Enums;
using HomeEase.Domain.Helpers;

namespace HomeEase.Application;

public class BookingMappingProfile : Profile
{
    public BookingMappingProfile()
    {
        CreateMap<Booking, BookingDto>()
            .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"))
            .ForMember(dest => dest.ProviderBusinessName, opt => opt.MapFrom<ProviderBusinessNameResolver>())
            .ForMember(dest => dest.TranslatedStatus, opt => opt.MapFrom<BookingStatusTranslationResolver>())
            .ForMember(dest => dest.ServicePrice, opt => opt.MapFrom(src => src.ServicePrice))
            .ForMember(dest => dest.DurationMinutes, opt => opt.MapFrom(src => src.DurationMinutes))
            .ForMember(dest => dest.ProviderImageUrl, opt => opt.MapFrom(src => src.Provider.ProfileImageUrl))
            .ForMember(dest => dest.ProviderLocationString, opt => opt.MapFrom(src => $"{src.Provider.Address.Country}، {src.Provider.Address.City}"))
            .ForMember(dest => dest.ServiceName, opt => opt.MapFrom<ServiceNameResolver>())
            .ForMember(dest => dest.SessionLocationType, opt => opt.MapFrom<SessionLocationTypeResolver>());

        CreateMap<PaymentInfo, PaymentInfoDto>();
        CreateMap<PaymentInfoDto, PaymentInfo>();
    }
}

public class BookingStatusTranslationResolver(ICurrentUserService currentUserService) : IValueResolver<Booking, BookingDto, string>
{
    public string Resolve(Booking source, BookingDto destination, string destMember, ResolutionContext context)
    {
        return EnumTranslations.TranslateBookingStatus(source.Status, currentUserService.Language);
    }
}

public class ProviderBusinessNameResolver(ICurrentUserService currentUserService) : IValueResolver<Booking, BookingDto, string>
{
    public string Resolve(Booking source, BookingDto destination, string destMember, ResolutionContext context)
    {
        return currentUserService.Language == LanguageEnum.Ar
            ? source.Provider.BusinessNameAr ?? source.Provider.BusinessName
            : source.Provider.BusinessName;
    }
}

public class ServiceNameResolver(ICurrentUserService currentUserService) : IValueResolver<Booking, BookingDto, string>
{
    public string Resolve(Booking source, BookingDto destination, string destMember, ResolutionContext context)
    {
        return currentUserService.Language == LanguageEnum.Ar
            ? source.Service.NameAr ?? source.Service.Name
            : source.Service.Name;
    }
}

public class SessionLocationTypeResolver(ICurrentUserService currentUserService) : IValueResolver<Booking, BookingDto, string>
{
    public string Resolve(Booking source, BookingDto destination, string destMember, ResolutionContext context)
    {
        return EnumTranslations.TranslateIsHomeService(source.IsHomeService, currentUserService.Language);
    }
}
