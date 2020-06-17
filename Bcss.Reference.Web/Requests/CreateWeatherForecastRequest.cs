using System;
using System.ComponentModel.DataAnnotations;
using Bcss.Reference.Web.Validators;
using Bcss.Reference.Web.ViewModels;

namespace Bcss.Reference.Web.Requests
{
    public class CreateWeatherForecastRequest
    {
        public LocationViewModel Location { get; set; }

        public DateTime Date { get; set; }

        [StringLength(1000, MinimumLength = 1, ErrorMessage = "Summary cannot be null or empty.")]
        public string Summary { get; set; }

        public double Temperature { get; set; }

        [TemperatureScale]
        public string Scale { get; set; }
    }
}