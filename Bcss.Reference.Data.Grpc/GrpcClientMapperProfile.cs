using AutoMapper;
using Bcss.Reference.Data.Grpc.TypeConverters;
using Bcss.Reference.Grpc;

namespace Bcss.Reference.Data.Grpc
{
    public class GrpcClientMapperProfile : Profile
    {
        public GrpcClientMapperProfile()
        {
            // All types must have mappings created for them using `CreateMap<>`.
            // If the two classes are compatible with the default mapping strategy of
            // convention-based property name matching, you can simple call this method
            // and be done. However, for convenience, AutoMapper also exposes the `ReverseMap()`
            // method which will additionally create the opposite mapping (in this case,
            // LocationData to Location). ReverseMap() only works as expected when relying
            // on convention-based mapping, so don't try to use it on classes where you
            // have to specify additional handling for properties or need to use type converters.
            CreateMap<Location, LocationData>().ReverseMap();

            CreateMap<WeatherForecast, WeatherForecastData>()
                .ConvertUsing<GrpcWeatherForecastToWeatherForecastDataTypeConverter>();

            CreateMap<WeatherForecastData, WeatherForecast>()
                .ConvertUsing<WeatherForecastDataToGrpcWeatherForecastTypeConverter>();
        }
    }
}