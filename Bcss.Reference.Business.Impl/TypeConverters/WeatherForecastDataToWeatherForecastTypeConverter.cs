using AutoMapper;
using Bcss.Reference.Data;
using Bcss.Reference.Domain;

namespace Bcss.Reference.Business.Impl.TypeConverters
{
    public class WeatherForecastDataToWeatherForecastTypeConverter : ITypeConverter<WeatherForecastData, WeatherForecast>
    {
        public WeatherForecast Convert(WeatherForecastData source, WeatherForecast destination, ResolutionContext context)
        {
            destination ??= new WeatherForecast();

            destination.Id = source.Id;
            destination.Summary = source.Summary;
            destination.Temperature = source.TemperatureC;
            destination.Date = source.Date;
            destination.Location = context.Mapper.Map<LocationData, Location>(source.Location);

            return destination;
        }
    }
}