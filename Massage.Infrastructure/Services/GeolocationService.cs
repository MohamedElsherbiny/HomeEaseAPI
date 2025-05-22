using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Massage.Application.Interfaces.Services;

namespace Massage.Infrastructure.Services;

public class GeolocationService(HttpClient _httpClient, IConfiguration _configuration) : IGeolocationService
{
    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Implementation of the Haversine formula for calculating distance between two points on Earth
        const double EarthRadiusKm = 6371.0;

        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
               Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
               Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    public async Task<(double Latitude, double Longitude)> GetCoordinatesFromAddressAsync(string address)
    {
        // In a real application, this would use a geocoding service like Google Maps, Here, etc.
        // For this example, we'll implement a simplified version that would need to be replaced in production

        var apiKey = _configuration["Geocoding:ApiKey"];
        var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={apiKey}";

        try
        {
            var response = await _httpClient.GetStringAsync(url);
            using var document = JsonDocument.Parse(response);

            var root = document.RootElement;
            var status = root.GetProperty("status").GetString();

            if (status == "OK")
            {
                var location = root.GetProperty("results")[0].GetProperty("geometry").GetProperty("location");
                var lat = location.GetProperty("lat").GetDouble();
                var lng = location.GetProperty("lng").GetDouble();

                return (lat, lng);
            }

            // Return default values if geocoding failed
            return (0, 0);
        }
        catch (Exception)
        {
            // Return default values in case of error
            return (0, 0);
        }
    }
}

