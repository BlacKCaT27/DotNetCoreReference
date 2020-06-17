using System;
using Bcss.Reference.Domain;

namespace Bcss.Reference.Data
{
    public class WeatherForecastData
    {
        public int Id { get; set; }

        public string Summary { get; set; }

        public DateTime Date { get; set; }

        public LocationData Location { get; set; }

        public Celsius TemperatureC { get; set; }
    }
}