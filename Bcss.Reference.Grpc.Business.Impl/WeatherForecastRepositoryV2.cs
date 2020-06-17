using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bcss.Reference.Grpc.Shared;
using Bcss.Reference.Grpc.Shared.Config;
using Microsoft.Extensions.Options;

namespace Bcss.Reference.Grpc.Business.Impl
{
    /// <summary>
    /// This class represents the "next-gen" repository for the legacy app. The name being appended with "V2" is simply
    /// to create a difference in name. Very little about this class is actually different from the "v1" implementation;
    /// it is merely here to provide an example of how one can utilize configuration for feature flagging.
    ///
    /// This implementation is identical to the V1 implementation with the exception that a configurable string
    /// is appended onto all weather forecast summaries to differentiate the records from the V1 repository.
    /// </summary>
    public class WeatherForecastRepositoryV2 : IWeatherForecastRepository
    {
        private static readonly string[] Summaries =
{
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        private static readonly IDictionary<int, WeatherForecastStoredData> WeatherForecastStorage = new ConcurrentDictionary<int, WeatherForecastStoredData>();
        private static bool _initialized;

        private readonly IOptionsSnapshot<V2RepositorySettings> _optionsSnapshot;

        public WeatherForecastRepositoryV2(IOptionsSnapshot<V2RepositorySettings> optionsSnapshot)
        {
            _optionsSnapshot = optionsSnapshot;
            SeedData();
        }

        private void SeedData()
        {
            if (_initialized)
            {
                return;
            }

            WeatherForecastStorage[1] = new WeatherForecastStoredData
            {
                Id = 1,
                Location = new LocationStoredData
                {
                    Id = 1,
                    Latitude = 42.166679f,
                    Longitude = -83.781319f,
                    Name = "Saline, MI" + _optionsSnapshot.Value.Suffix
                },
                Date = "5/29/2020",
                Summary = Summaries[new Random().Next(0, Summaries.Length)],
                TemperatureC = 20
            };

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
                Summary = summary + _optionsSnapshot.Value.Suffix,
                TemperatureC = temperatureC
            };

            WeatherForecastStorage[id] = forecast;

            return Task.FromResult(forecast);
        }

        public Task<WeatherForecastStoredData> UpdateWeatherForecast(WeatherForecastStoredData weatherForecast)
        {
            weatherForecast.Summary += _optionsSnapshot.Value.Suffix;
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