namespace Bcss.Reference.Grpc.Shared
{
    public class WeatherForecastStoredData
    {
        public int Id { get; set; }

        public string Summary { get; set; }

        public string Date { get; set; }

        public LocationStoredData Location { get; set; }

        public double TemperatureC { get; set; }
    }
}