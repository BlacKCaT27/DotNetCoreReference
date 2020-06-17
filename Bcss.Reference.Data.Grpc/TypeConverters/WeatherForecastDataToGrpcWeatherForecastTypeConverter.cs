using AutoMapper;
using Bcss.Reference.Domain;
using Location = Bcss.Reference.Grpc.Location;
using WeatherForecast = Bcss.Reference.Grpc.WeatherForecast;

namespace Bcss.Reference.Data.Grpc.TypeConverters
{
    public class WeatherForecastDataToGrpcWeatherForecastTypeConverter : ITypeConverter<WeatherForecastData, WeatherForecast>
    {
        public WeatherForecast Convert(WeatherForecastData source, WeatherForecast destination, ResolutionContext context)
        {
            destination ??= new WeatherForecast();

            destination.Id = source.Id;
            destination.Summary = source.Summary;
            destination.Temperature = (double)source.TemperatureC.Value;
            destination.Location = context.Mapper.Map<LocationData, Location>(source.Location);
            destination.Date = source.Date.ToMonthDayYearDate();

            return destination;
        }
    }
}