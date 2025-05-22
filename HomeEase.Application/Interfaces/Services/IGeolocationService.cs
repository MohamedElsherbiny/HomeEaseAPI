namespace HomeEase.Application.Interfaces.Services
{
    public interface IGeolocationService
    {
        double CalculateDistance(double lat1, double lon1, double lat2, double lon2);
        Task<(double Latitude, double Longitude)> GetCoordinatesFromAddressAsync(string address);
    }
}
