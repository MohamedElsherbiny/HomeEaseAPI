using HomeEase.Domain.Enums;

namespace HomeEase.Domain.Helpers;

public static class EnumTranslations
{
    public static string TranslateBookingStatus(BookingStatus status, LanguageEnum language)
    {
        if (language == LanguageEnum.Ar)
        {
            return status switch
            {
                BookingStatus.Pending => "قيد الانتظار",
                BookingStatus.Confirmed => "تم القبول",
                BookingStatus.Completed => "مكتملة",
                BookingStatus.Cancelled => "ملغاة",
                _ => throw new NotImplementedException($"Status {status} not found")
            };
        }

        return status.ToString();
    }

    public static string TranslateIsHomeService(bool ishomeService, LanguageEnum language)
    {
        return ishomeService
            ? (language is LanguageEnum.Ar ? "بالمنزل" : "At Home")
            : (language is LanguageEnum.Ar ? "بالمركز" : "At Center");
    }
}
