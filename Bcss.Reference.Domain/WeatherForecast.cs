using System;

namespace Bcss.Reference.Domain
{
    public class WeatherForecast
    {
        public int Id { get; set; }

        public Location Location { get; set; }

        public DateTime Date { get; set; }

        public Temperature Temperature { get; set; }

        public string Summary { get; set; }
    }
}
