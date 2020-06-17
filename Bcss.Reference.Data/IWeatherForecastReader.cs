using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bcss.Reference.Data
{
    public interface IWeatherForecastReader
    {
        Task<ICollection<WeatherForecastData>> ListWeatherForecasts();

        Task<WeatherForecastData> GetWeatherForecastById(int weatherForecastId);

        Task<WeatherForecastData> GetWeatherForecastByDate(int locationId, string date);
    }
}