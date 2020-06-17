using AutoMapper;
using Bcss.Reference.Data;
using Bcss.Reference.Domain;

namespace Bcss.Reference.Business.Impl.TypeConverters
{
    /// <summary>
    /// Class responsible for converting WeatherForecast objects to WeatherForecastData objects.
    ///
    /// TypeConverters are how we can hook into AutoMappers mapping system when converting between two types whose
    /// fields don't map cleanly on their own. By default, AutoMapper uses a convention-based mapping strategy
    /// where properties with matching names between two objects are automatically mapped. In cases like this
    /// where the shape of the objects differ, using a TypeConverter allows you to still map the objects by hand.
    /// When possible, you should attempt to structure your data so you don't need these, but it's more important that the
    /// data is shaped best for its use. Don't ever compromise on the shape of data just to make type converting easier.
    /// </summary>
    public class WeatherForecastToWeatherForecastDataTypeConverter : ITypeConverter<WeatherForecast, WeatherForecastData>
    {
        /// <summary>
        /// Responsible for converting the source WeatherForecast object to a WeatherForecastData object.
        /// </summary>
        /// <param name="source">The object to be converted from.</param>
        /// <param name="destination">The object to be populated with data.</param>
        /// <param name="context">An object containing contextual information that may be needed to help with conversions.
        /// This includes a reference to an <see cref="IMapper"/> object to map child objects as needed.</param>
        /// <returns></returns>
        public WeatherForecastData Convert(WeatherForecast source, WeatherForecastData destination, ResolutionContext context)
        {
            // In practice, AutoMapper has been known to pass a null destination at times, so guard for that.
            if (destination == null)
            {
                destination = new WeatherForecastData();
            }

            destination.Id = source.Id;
            destination.Location = context.Mapper.Map<Location, LocationData>(source.Location);
            destination.Date = source.Date;
            destination.TemperatureC = source.Temperature.ToCelsius();
            destination.Summary = source.Summary;

            return destination;
        }
    }
}