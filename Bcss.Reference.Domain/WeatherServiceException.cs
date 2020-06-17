using System;

namespace Bcss.Reference.Domain
{
    // This marker class is the base exception class all exceptions thrown by the system should inherit from.
    public abstract class WeatherServiceException : Exception
    {
        protected WeatherServiceException(string message) : base(message)
        {
        }
    }
}