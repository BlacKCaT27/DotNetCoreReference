namespace Bcss.Reference.Domain
{
    public class LocationCreationException : WeatherServiceException
    {
        public LocationCreationException(string message) : base(message)
        {
        }
    }
}