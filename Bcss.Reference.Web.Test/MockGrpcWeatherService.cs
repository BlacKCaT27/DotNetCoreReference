using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bcss.Reference.Data;
using Bcss.Reference.Domain;

namespace Bcss.Reference.Web.Test
{
    public class MockGrpcWeatherService : IWeatherForecastReader, IWeatherForecastWriter
    {
        private static readonly IDictionary<int, WeatherForecastData> WeatherForecastStorage = new ConcurrentDictionary<int, WeatherForecastData>();

        public MockGrpcWeatherService(Action<IDictionary<int, WeatherForecastData>> seedData)
        {
            seedData?.Invoke(WeatherForecastStorage);
        }

        public Task<WeatherForecastData> GetWeatherForecastById(int id)
        {
            return Task.FromResult(WeatherForecastStorage[id]);
        }

        public Task<WeatherForecastData> GetWeatherForecastByDate(int locationId, string date)
        {
            WeatherForecastData forecast = null;
            foreach (var weatherForecast in WeatherForecastStorage.Values)
            {
                if (weatherForecast.Location.Id == locationId &&
                    weatherForecast.Date == DateTime.Parse(date))
                {
                    forecast = weatherForecast;
                }
            }

            return Task.FromResult(forecast);
        }

        public Task<ICollection<WeatherForecastData>> ListWeatherForecasts()
        {
            return Task.FromResult(WeatherForecastStorage.Values);
        }

        public Task<WeatherForecastData> CreateWeatherForecast(LocationData location, string date, string summary, decimal temperatureC)
        {
            var id = GetNextId();
            var forecast = new WeatherForecastData
            {
                Id = id,
                Location = location,
                Date = DateTime.Parse(date),
                Summary = summary,
                TemperatureC = new Celsius(temperatureC)
            };

            WeatherForecastStorage[id] = forecast;

            return Task.FromResult(forecast);
        }

        public Task<WeatherForecastData> UpdateWeatherForecast(WeatherForecastData weatherForecast)
        {
            WeatherForecastStorage[weatherForecast.Id] = weatherForecast;
            return Task.FromResult(weatherForecast);
        }

        public Task<bool> DeleteWeatherForecast(int weatherForecastId)
        {
            var removed = WeatherForecastStorage.Remove(weatherForecastId);
            return Task.FromResult(removed);
        }

        private static int GetNextId()
        {
            var nextId = 0;

            foreach (var key in WeatherForecastStorage.Keys)
            {
                if (key > nextId)
                {
                    nextId = key;
                }
            }

            nextId += 1;

            return nextId;
        }
    }
}