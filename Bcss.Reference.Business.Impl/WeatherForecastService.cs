using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bcss.Reference.Data;
using Bcss.Reference.Domain;

namespace Bcss.Reference.Business.Impl
{
    /// <summary>
    /// The service layer is where the bulk of all business logic around data should exist.
    /// Here is where we can handle things like converting DateTimes to strings, and converting temperatures between
    /// the different types. All depending on what the business rules are. Everything outside of this class should be considered
    /// a 'necessary evil' for meeting the businesses needs. If there is ever a need to change business rules, it should be done
    /// here and no where else.
    ///
    /// It's important to keep in mind that the business rules are not just found within the methods of service layer classes such
    /// as `WeatherForecastService`. A key important reason to include this layer is to represent the business domain of your application.
    /// That includes domain objects and any validations or rules they may enforce. All of those enforced actions are also business rules,
    /// because they describe the process that is being performed from the perspective of the business, not any internal mechanism required
    /// for the code to run. This can be a bit tricky to feel out at times, and does require you to try to conceptualize the code in a somewhat
    /// abstract way mentally, but developing that skill is critical to being able to properly identify and enforce the proper layering of an application.
    /// </summary>
    public class WeatherForecastService : IWeatherForecastService
    {
        private readonly IWeatherForecastReader _reader;
        private readonly IWeatherForecastWriter _writer;

        private readonly IMapper _mapper;

        public WeatherForecastService(IWeatherForecastReader reader, IWeatherForecastWriter writer, IMapper mapper)
        {
            _reader = reader;
            _writer = writer;
            _mapper = mapper;
        }

        /// <summary>
        /// Creates a new weather forecast record for the given location and time.
        /// </summary>
        /// <param name="location">The location of where the weather forecast is for.</param>
        /// <param name="date">The date the forecast applies to.</param>
        /// <param name="temperature">This is the most important parameter in the entire application.</param>
        /// <param name="scale">The scale of the temperature value being passed in.</param>
        /// <param name="summary">A summary explaining the conditions of the forecast.</param>
        /// <returns></returns>
        public async Task<WeatherForecast> CreateWeatherForecast(Location location, DateTime date, decimal temperature, string scale, string summary)
        {
            var locationData = _mapper.Map<Location, LocationData>(location);
            var temperatureInC = Temperature.FromValue(temperature, scale).ToCelsius().Value;

            var result = await _writer.CreateWeatherForecast(locationData, date.ToMonthDayYearDate(),
                summary, temperatureInC);

            return _mapper.Map<WeatherForecastData, WeatherForecast>(result);
        }

        public async Task<WeatherForecast> GetWeatherForecastForDate(int locationId, DateTime date)
        {
            var dateStr = date.ToMonthDayYearDate();
            var weatherForecastData = await _reader.GetWeatherForecastByDate(locationId, dateStr);
            return _mapper.Map<WeatherForecastData, WeatherForecast>(weatherForecastData);
        }

        public async Task<WeatherForecast> GetWeatherForecastById(int weatherForecastId)
        {
            var weatherForecastData = await _reader.GetWeatherForecastById(weatherForecastId);
            return _mapper.Map<WeatherForecastData, WeatherForecast>(weatherForecastData);
        }

        public async Task<IEnumerable<WeatherForecast>> ListWeatherForecasts()
        {
            var weatherForecastDataCollection = await _reader.ListWeatherForecasts();
            return weatherForecastDataCollection.Select(wf => _mapper.Map<WeatherForecastData, WeatherForecast>(wf))
                .ToList();
        }

        public async Task<WeatherForecast> UpdateWeatherForecast(WeatherForecast forecast)
        {
            var forecastData = _mapper.Map<WeatherForecast, WeatherForecastData>(forecast);
            var result = await _writer.UpdateWeatherForecast(forecastData);
            return _mapper.Map<WeatherForecastData, WeatherForecast>(result);
        }

        public async Task<bool> DeleteWeatherForecast(int weatherForecastId)
        {
            return await _writer.DeleteWeatherForecast(weatherForecastId);
        }
    }
}
