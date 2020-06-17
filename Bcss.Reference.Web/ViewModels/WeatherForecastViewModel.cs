using System;
using System.ComponentModel.DataAnnotations;
using Bcss.Reference.Web.Validators;

namespace Bcss.Reference.Web.ViewModels
{
    /// <summary>
    /// ViewModels (for lack of a better term) represent the models of an application
    /// as represented by their consumers. These objects are mapped from domain objects
    /// but only expose the bare minimum amount of information needed by consumers.
    ///
    /// This is done to ensure that the internal models of an application can change over time
    /// as needed without those changes having a potential impact on consumers.
    /// </summary>
    public class WeatherForecastViewModel
    {
        public int Id { get; set; }

        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Summary cannot be null or empty.")]
        public string Summary { get; set; }

        public DateTime Date { get; set; }

        public decimal Temperature { get; set; }

        [TemperatureScale]
        public string Scale { get; set; }

        public LocationViewModel Location { get; set; }
    }
}