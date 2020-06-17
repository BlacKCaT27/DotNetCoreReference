using System;
using AutoMapper;
using Bcss.Reference.Domain;
using WeatherForecast = Bcss.Reference.Grpc.WeatherForecast;

namespace Bcss.Reference.Data.Grpc.TypeConverters
{
    public class GrpcWeatherForecastToWeatherForecastDataTypeConverter : ITypeConverter<WeatherForecast, WeatherForecastData>
    {
        public WeatherForecastData Convert(WeatherForecast source, WeatherForecastData destination, ResolutionContext context)
        {
            destination ??= new WeatherForecastData();

            destination.Id = source.Id;

            // HACK: This is purely due to the use of strings for date representations which use a month/day/year format for
            // readability and demo purposes. In a real application, dates should be transmitted over gRPC using the google/protobuf/Timestamp
            // well-known type, which provides converters to and from DateTime objects. Don't ever actually store dates like this.
            var dateParts = source.Date.Split('/');
            destination.Date = new DateTime(int.Parse(dateParts[2]), int.Parse(dateParts[0]), int.Parse(dateParts[1]));

            destination.Summary = source.Summary;
            destination.TemperatureC = new Celsius((decimal)source.Temperature);

            destination.Location = context.Mapper.Map<Reference.Grpc.Location, LocationData>(source.Location);

            return destination;
        }
    }
}