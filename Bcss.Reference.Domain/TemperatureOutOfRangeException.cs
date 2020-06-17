namespace Bcss.Reference.Domain
{
    public class TemperatureOutOfRangeException : WeatherServiceException
    {
        public decimal Value { get; }

        public Scale Scale { get; }

        public TemperatureOutOfRangeException(decimal value, Scale scale, string message) : base(message)
        {
            Value = value;
            Scale = scale;
        }
    }
}