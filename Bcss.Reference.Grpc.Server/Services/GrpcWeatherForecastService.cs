using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bcss.Reference.Grpc.Business;
using Bcss.Reference.Grpc.Shared;
using Bcss.Reference.Grpc.Shared.Config;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

namespace Bcss.Reference.Grpc.Server.Services
{
    public class GrpcWeatherForecastService : WeatherService.WeatherServiceBase
    {
        private readonly ILogger<GrpcWeatherForecastService> _logger;
        private readonly IWeatherForecastRepository _repository;
        private readonly IMapper _mapper;
        private readonly IFeatureManager _featureManager;

        public GrpcWeatherForecastService(
            ILogger<GrpcWeatherForecastService> logger,
            IWeatherForecastRepository repository,
            IMapper mapper,
            IFeatureManager featureManager)
        {
            _logger = logger;
            _repository = repository;
            _mapper = mapper;
            _featureManager = featureManager;
        }

        public override async Task<CreateWeatherForecastResponse> CreateWeatherForecast(CreateWeatherForecastRequest request, ServerCallContext context)
        {
            var locationId = request.Location.Id;
            var date = request.Date;

            if (locationId <= 0)
            {
                const string error = "Location id must be a positive integer";
                throw new RpcException(new Status(StatusCode.InvalidArgument, error));
            }

            if (string.IsNullOrEmpty(date))
            {
                const string error = "date cannot be null or empty.";
                throw new RpcException(new Status(StatusCode.InvalidArgument, error));
            }

            var locationStoredData = _mapper.Map<Location, LocationStoredData>(request.Location);

            var forecast = await _repository.CreateWeatherForecast(locationStoredData, date, request.Summary, request.Temperature);

            var result = _mapper.Map<WeatherForecastStoredData, WeatherForecast>(forecast);

            return new CreateWeatherForecastResponse
            {
                WeatherForecast = result
            };
        }

        public override async Task<ListWeatherForecastsResponse> ListWeatherForecasts(ListWeatherForecastRequest request, ServerCallContext context)
        {
            var forecasts = await _repository.ListWeatherForecasts();

            var results = forecasts.Select(f => _mapper.Map<WeatherForecastStoredData, WeatherForecast>(f));

            return new ListWeatherForecastsResponse
            {
                Forecasts = { results }
            };
        }

        public override async Task<GetWeatherForecastResponse> GetWeatherForecastById(GetWeatherForecastByIdRequest request, ServerCallContext context)
        {
            var id = request.WeatherForecastId;

            if (id <= 0)
            {
                const string error = "Weather Forecast id must be a positive integer";
                throw new RpcException(new Status(StatusCode.InvalidArgument, error));
            }

            var result = await _repository.GetWeatherForecastById(id);

            return new GetWeatherForecastResponse
            {
                Forecast = _mapper.Map<WeatherForecastStoredData, WeatherForecast>(result)
            };
        }
        
        public override async Task<GetWeatherForecastResponse> GetWeatherForecastByDate(GetWeatherForecastByDateRequest request, ServerCallContext context)
        {
            if (await _featureManager.IsEnabledAsync(nameof(FeatureFlags.AllowGetForecastByDate)))
            {
                _logger.LogInformation("Now handling weather forecast request for location '{location}' at date '{date}'", request.LocationId, request.Date);

                var locationId = request.LocationId;
                var date = request.Date;

                if (locationId <= 0)
                {
                    const string error = "Location id must be a positive integer";
                    throw new RpcException(new Status(StatusCode.InvalidArgument, error));
                }

                if (string.IsNullOrEmpty(date))
                {
                    const string error = "date cannot be null or empty.";
                    throw new RpcException(new Status(StatusCode.InvalidArgument, error));
                }

                var forecast = await _repository.GetWeatherForecastByDate(locationId, date);

                if (forecast == null)
                {
                    var error = $"The forecast at location {locationId} on date {date} could not be found.";
                    _logger.LogError(error);
                    throw new RpcException(new Status(StatusCode.NotFound, error));
                }

                var result = _mapper.Map<WeatherForecastStoredData, WeatherForecast>(forecast);

                return new GetWeatherForecastResponse
                {
                    Forecast = result
                };
            }

            throw new RpcException(new Status(StatusCode.Unimplemented, "This feature is disabled. Please contact a system administrator for more information."));
        }

        public override async Task<UpdateWeatherForecastResponse> UpdateWeatherForecast(UpdateWeatherForecastRequest request, ServerCallContext context)
        {
            var forecastId = request.Forecast.Id;
            var date = request.Forecast.Date;
            var locationId = request.Forecast.Location.Id;

            if (forecastId <= 0)
            {
                const string error = "Forecast id must be a positive integer";
                throw new RpcException(new Status(StatusCode.InvalidArgument, error));
            }

            if (string.IsNullOrEmpty(date))
            {
                const string error = "date cannot be null or empty.";
                throw new RpcException(new Status(StatusCode.InvalidArgument, error));
            }

            if (locationId <= 0)
            {
                const string error = "Location id must be a positive integer";
                throw new RpcException(new Status(StatusCode.InvalidArgument, error));
            }

            var weatherForecast = request.Forecast;

            var weatherForecastStoredData = _mapper.Map<WeatherForecast, WeatherForecastStoredData>(weatherForecast);

            var updatedData = await _repository.UpdateWeatherForecast(weatherForecastStoredData);

            var result = _mapper.Map<WeatherForecastStoredData, WeatherForecast>(updatedData);

            return new UpdateWeatherForecastResponse
            {
                Forecast = result
            };
        }

        public override async Task<DeleteWeatherForecastResponse> DeleteWeatherForecast(DeleteWeatherForecastRequest request, ServerCallContext context)
        {
            var weatherForecastId = request.Id;
            var result = await _repository.DeleteWeatherForecast(weatherForecastId);
            return new DeleteWeatherForecastResponse
            {
                Deleted = result
            };
        }
    }
}
