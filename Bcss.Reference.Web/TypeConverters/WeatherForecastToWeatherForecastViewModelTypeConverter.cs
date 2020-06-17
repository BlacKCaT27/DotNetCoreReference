using AutoMapper;
using Bcss.Reference.Domain;
using Bcss.Reference.Web.ViewModels;

namespace Bcss.Reference.Web.TypeConverters
{
    public class WeatherForecastToWeatherForecastViewModelTypeConverter : ITypeConverter<WeatherForecast, WeatherForecastViewModel>
    {
        public WeatherForecastViewModel Convert(WeatherForecast source, WeatherForecastViewModel destination,
            ResolutionContext context)
        {
            destination ??= new WeatherForecastViewModel();

            destination.Id = source.Id;
            destination.Date = source.Date;
            destination.Summary = source.Summary;
            destination.Temperature = source.Temperature.Value;
            destination.Scale = nameof(source.Temperature.Scale);
            destination.Location = context.Mapper.Map<Location, LocationViewModel>(source.Location);

            return destination;
        }
    }
}