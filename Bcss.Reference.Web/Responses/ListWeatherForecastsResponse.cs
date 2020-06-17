using System.Collections.Generic;
using Bcss.Reference.Web.ViewModels;

namespace Bcss.Reference.Web.Responses
{
    public class ListWeatherForecastsResponse
    {
        /// <summary>
        /// Gets or sets the list of <see cref="WeatherForecastViewModel"/> objects to be returned to the caller.
        ///
        /// There are two things to note here:
        ///     1) We're returning an interface collection called "IEnumerable". This is the most generic form of collection provided
        /// in .Net Core. You should always aim to return any collection of objects from any interface or class with an IEnumerable
        /// type. Internally, you can return a concrete collection type since they all implement IEnumerable, but by sticking with
        /// the most generic collection type, you get several benefits, including immutability, lazy data population, and the flexibility
        /// to utilize streams to improve performance where needed.
        ///
        ///     2) We use a default initializer to ensure that this property starts its life off as an empty `List` type.
        /// A good pattern to follow for any collection property or field is to initialize the property with an empty collection
        /// of some kind (whatever concrete type is most appropriate for your use case). This ensures that no readers or writers of
        /// the collection need to worry about handling null lists, which help keeps procedural logic out of your codebase and allows
        /// for a more natural read of the code.
        /// </summary>
        public IEnumerable<WeatherForecastViewModel> WeatherForecasts { get; set; } = new List<WeatherForecastViewModel>();
    }
}