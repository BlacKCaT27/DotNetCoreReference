using AutoMapper;
using Bcss.Reference.Business.Impl.TypeConverters;
using Bcss.Reference.Data;
using Bcss.Reference.Domain;

namespace Bcss.Reference.Business.Impl
{
    public class BusinessMapperProfile : Profile
    {
        public BusinessMapperProfile()
        {
            // Here, we're telling AutoMapper that rather than using its built-in convention-based
            // strategy for handling objects conversions, to use the TypeConverter we wrote
            // to handle it.
            CreateMap<WeatherForecast, WeatherForecastData>()
                .ConvertUsing<WeatherForecastToWeatherForecastDataTypeConverter>();

            CreateMap<WeatherForecastData, WeatherForecast>()
                .ConvertUsing<WeatherForecastDataToWeatherForecastTypeConverter>();

            CreateMap<Location, LocationData>()
                .ReverseMap();

            // We can also supply type converters in-line as a delegate.
            CreateMap<decimal, Celsius>()
                .ConvertUsing((source, destination) => new Celsius(source));
        }
    }
}