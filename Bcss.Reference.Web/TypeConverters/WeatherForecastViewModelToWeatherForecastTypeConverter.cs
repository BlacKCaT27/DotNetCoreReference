using System;
using AutoMapper;
using Bcss.Reference.Domain;
using Bcss.Reference.Web.ViewModels;

namespace Bcss.Reference.Web.TypeConverters
{
    public class WeatherForecastViewModelToWeatherForecastTypeConverter : ITypeConverter<WeatherForecastViewModel, WeatherForecast>
    {
        public WeatherForecast Convert(WeatherForecastViewModel source, WeatherForecast destination, ResolutionContext context)
        {
            destination ??= new WeatherForecast();

            destination.Id = source.Id;
            destination.Date = source.Date;
            destination.Summary = source.Summary;
            destination.Temperature = Temperature.FromValue(source.Temperature, source.Scale);
            destination.Location = context.Mapper.Map<LocationViewModel, Location>(source.Location);

            return destination;
        }
    }
}