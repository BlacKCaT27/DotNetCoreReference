using System.Collections.Generic;
using System.Threading.Tasks;
using Bcss.Reference.Grpc.Shared;

namespace Bcss.Reference.Grpc.Business
{
    /// <summary>
    /// Represents a repository in which WeatherForecast data can be stored.
    ///
    /// Note that this interface intentionally violates the Interface Segregation principle to
    /// help show an example of one of the many methods to implement feature flagging.
    /// </summary>
    public interface IWeatherForecastRepository
    {
        Task<ICollection<WeatherForecastStoredData>> ListWeatherForecasts();

        Task<WeatherForecastStoredData> GetWeatherForecastById(int weatherForecastId);

        Task<WeatherForecastStoredData> GetWeatherForecastByDate(int locationId, string date);

        Task<WeatherForecastStoredData> CreateWeatherForecast(LocationStoredData location, string date, string summary,
            double temperatureC);

        Task<WeatherForecastStoredData> UpdateWeatherForecast(WeatherForecastStoredData weatherForecast);

        Task<bool> DeleteWeatherForecast(int weatherForecastId);
    }
}