using System.Threading.Tasks;

namespace Bcss.Reference.Data
{
    public interface IWeatherForecastWriter
    {
        Task<WeatherForecastData> CreateWeatherForecast(LocationData location, string date, string summary,
            decimal temperatureC);

        Task<WeatherForecastData> UpdateWeatherForecast(WeatherForecastData weatherForecast);

        Task<bool> DeleteWeatherForecast(int weatherForecastId);
    }
}