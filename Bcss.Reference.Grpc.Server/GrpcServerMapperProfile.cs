using AutoMapper;
using Bcss.Reference.Grpc.Shared;

namespace Bcss.Reference.Grpc.Server
{
    public class GrpcServerMapperProfile : Profile
    {
        public GrpcServerMapperProfile()
        {
            CreateMap<Location, LocationStoredData>().ReverseMap();

            // The ForMember() method allows us to manually map individual field names if they do not match.
            CreateMap<WeatherForecast, WeatherForecastStoredData>()
                .ForMember(
                    dest => dest.TemperatureC,
                    opt => opt.MapFrom(
                        src => src.Temperature))
                .ReverseMap();
        }
    }
}