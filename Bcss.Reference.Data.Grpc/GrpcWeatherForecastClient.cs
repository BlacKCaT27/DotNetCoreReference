using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bcss.Reference.Grpc;

namespace Bcss.Reference.Data.Grpc
{
    public class GrpcWeatherForecastClient : IWeatherForecastReader, IWeatherForecastWriter
    {
        private readonly WeatherService.WeatherServiceClient _client;
        private readonly IMapper _mapper;

        public GrpcWeatherForecastClient(WeatherService.WeatherServiceClient client, IMapper mapper)
        {
            _client = client;
            _mapper = mapper;
        }

        public async Task<WeatherForecastData> CreateWeatherForecast(LocationData locationData, string date, string summary, decimal temperature)
        {
            var location = _mapper.Map<LocationData, Location>(locationData);
            var request = new CreateWeatherForecastRequest
            {
                Location = location,
                Date = date,
                Summary = summary,
                Temperature = (double)temperature
            };

            var response = await _client.CreateWeatherForecastAsync(request);
            return _mapper.Map<WeatherForecast, WeatherForecastData>(response.WeatherForecast);
        }

        public async Task<WeatherForecastData> GetWeatherForecastByDate(int locationId, string date)
        {
            var request = new GetWeatherForecastByDateRequest
            {
                LocationId = locationId,
                Date = date
            };

            var response = await _client.GetWeatherForecastByDateAsync(request);
            return _mapper.Map<WeatherForecast, WeatherForecastData>(response.Forecast);
        }

        public async Task<WeatherForecastData> GetWeatherForecastById(int id)
        {
            var request = new GetWeatherForecastByIdRequest
            {
                WeatherForecastId = id
            };

            var response = await _client.GetWeatherForecastByIdAsync(request);
            return _mapper.Map<WeatherForecast, WeatherForecastData>(response.Forecast);
        }

        public async Task<ICollection<WeatherForecastData>> ListWeatherForecasts()
        {
            var request = new ListWeatherForecastRequest();

            var response = await _client.ListWeatherForecastsAsync(request);
            return response.Forecasts.Select(f => _mapper.Map<WeatherForecast, WeatherForecastData>(f)).ToList();
        }

        public async Task<WeatherForecastData> UpdateWeatherForecast(WeatherForecastData weatherForecastData)
        {
            var forecast = _mapper.Map<WeatherForecastData, WeatherForecast>(weatherForecastData);

            var request = new UpdateWeatherForecastRequest
            {
                Forecast = forecast
            };

            var response = await _client.UpdateWeatherForecastAsync(request);
            return _mapper.Map<WeatherForecast, WeatherForecastData>(response.Forecast);
        }

        public async Task<bool> DeleteWeatherForecast(int weatherForecastId)
        {
            var request = new DeleteWeatherForecastRequest
            {
                Id =  weatherForecastId
            };

            return (await _client.DeleteWeatherForecastAsync(request)).Deleted;
        }
    }
}
