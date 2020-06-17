using AutoMapper;
using Bcss.Reference.Domain;
using Bcss.Reference.Web.TypeConverters;
using Bcss.Reference.Web.ViewModels;

namespace Bcss.Reference.Web
{
    public class WebMapperProfile : Profile
    {
        public WebMapperProfile()
        {
            CreateMap<WeatherForecast, WeatherForecastViewModel>()
                .ConvertUsing<WeatherForecastToWeatherForecastViewModelTypeConverter>();

            CreateMap<WeatherForecastViewModel, WeatherForecast>()
                .ConvertUsing<WeatherForecastViewModelToWeatherForecastTypeConverter>();
            
            CreateMap<Location, LocationViewModel>().ReverseMap();
            CreateMap<LocationViewModel, Location>()
                .ConvertUsing<LocationViewModelToLocationTypeConverter>();
        }
    }
}