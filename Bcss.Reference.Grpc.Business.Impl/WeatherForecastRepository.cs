using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bcss.Reference.Grpc.Shared;

namespace Bcss.Reference.Grpc.Business.Impl
{
    public class WeatherForecastRepository : IWeatherForecastRepository
    {
        private static readonly string[] Summaries =
{
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        private static readonly IDictionary<int, WeatherForecastStoredData> WeatherForecastStorage = new ConcurrentDictionary<int, WeatherForecastStoredData>();
        private static bool _initialized = false;

        public WeatherForecastRepository()
        {
            if (_initialized)
            {
                return;
            }
            SeedData();
        }

        private static void SeedData()
        {
            WeatherForecastStorage.Add(1, new WeatherForecastStoredData
            {
                Id = 1,
                Location = new LocationStoredData
                {
                    Id = 1,
                    Latitude = 42.166679f,
                    Longitude = -83.781319f,
                    Name = "Saline, MI"
                },
                Date = "5/29/2020",
                Summary = Summaries[new Random().Next(0, Summaries.Length)],
                TemperatureC = 20
            });

            _initialized = true;
        }

        public Task<WeatherForecastStoredData> GetWeatherForecastById(int id)
        {
            return Task.FromResult(WeatherForecastStorage[id]);
        }

        public Task<WeatherForecastStoredData> GetWeatherForecastByDate(int locationId, string date)
        {
            WeatherForecastStoredData forecast = null;
            foreach (var weatherForecast in WeatherForecastStorage.Values)
            {
                if (weatherForecast.Location.Id == locationId &&
                    weatherForecast.Date == date)
                {
                    forecast = weatherForecast;
                }
            }

            return Task.FromResult(forecast);
        }

        public Task<ICollection<WeatherForecastStoredData>> ListWeatherForecasts()
        {
            return Task.FromResult(WeatherForecastStorage.Values);
        }

        public Task<WeatherForecastStoredData> CreateWeatherForecast(LocationStoredData location, string date, string summary, double temperatureC)
        {
            var id = GetNextId();
            var forecast = new WeatherForecastStoredData
            {
                Id = id,
                Location = location,
                Date = date,
                Summary = summary,
                TemperatureC = temperatureC
            };

            WeatherForecastStorage[id] = forecast;

            return Task.FromResult(forecast);
        }

        public Task<WeatherForecastStoredData> UpdateWeatherForecast(WeatherForecastStoredData weatherForecast)
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