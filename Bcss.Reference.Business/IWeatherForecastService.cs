using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bcss.Reference.Domain;

namespace Bcss.Reference.Business
{
    /// <summary>
    /// The IWeatherForecastService provides weather forecasts based on the given date.
    /// </summary>
    public interface IWeatherForecastService
    {
        Task<WeatherForecast> CreateWeatherForecast(Location location, DateTime date, decimal temperature, string scale, string summary);

        Task<WeatherForecast> GetWeatherForecastForDate(int locationId, DateTime date);

        Task<WeatherForecast> GetWeatherForecastById(int weatherForecastId);

        Task<IEnumerable<WeatherForecast>> ListWeatherForecasts();

        Task<WeatherForecast> UpdateWeatherForecast(WeatherForecast weatherForecast);

        Task<bool> DeleteWeatherForecast(int weatherForecastId);
    }
}