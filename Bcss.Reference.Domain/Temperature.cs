using System;

namespace Bcss.Reference.Domain
{
    /// <summary>
    /// Temperature serves as the base class for a series of temperature related classes.
    /// These classes are referred to as "Value Objects". These objects represent a value
    /// of a specific type that can contain validation rules that must be met in order
    /// for the object to be instantiated. This is different from the typical rules
    /// for constructors which dictate they should be free of logic and do little more
    /// than initialize member variables with injected dependencies.
    ///
    /// But the intent here is to represent a value in such a way that gives it a
    /// semantic meaning at a business level. Our application deals with weather data,
    /// which includes temperatures of different scales. It's very helpful, at a
    /// business domain level, to have concrete types that represent values we can
    /// trust to be correct, and not just any possible value from a request or query.
    ///
    /// In the case of temperatures, having concrete types for the various scales also
    /// allows us to write convenient helper methods to navigate between scales as needed.
    /// </summary>
    public abstract class Temperature
    {
        protected decimal MinValue { get; set; }

        // If weather forecasts ever start needing to validate against the Planck constant,
        // we have bigger problems, so just use double.MaxValue as a safe alternative to
        // using scientific notation.
        private decimal MaxValue { get; } = decimal.MaxValue;

        /// <summary>
        /// Gets or sets the Scale this temperature was measured in. Note that only subclasses
        /// can set this value.
        /// </summary>
        public Scale Scale { get; protected set; }

        /// <summary>
        /// Gets or sets the Value of this temperature. Note that only subclasses can set this value.
        /// </summary>
        public decimal Value { get; protected set; }

        public static Temperature FromValue(decimal value, string scaleName)
        {
            var fullScaleName = scaleName switch
            {
                "c" => "celsius",
                "f" => "fahrenheit",
                "k" => "kelvin",
                _ => scaleName
            };
            Enum.TryParse<Scale>(fullScaleName, true, out var scale);

            return scale switch
            {
                Scale.Celsius => new Celsius(value),
                Scale.Fahrenheit => new Fahrenheit(value),
                Scale.Kelvin => new Kelvin(value),
                Scale.Unknown => throw new ArgumentOutOfRangeException(nameof(scale)),
                _ => throw new ArgumentOutOfRangeException(nameof(scale))
            };
        }

        protected void Validate()
        {
            if (Value < MinValue)
            {
                throw new TemperatureOutOfRangeException(Value, Scale, $"Temperature value {Value} is below the minimum value for a temperature reading using the {Scale} scale.");
            }

            if (Value > MaxValue)
            {
                throw new TemperatureOutOfRangeException(Value, Scale, $"Temperature value {Value} is below the minimum value for a temperature reading using the {Scale} scale.");
            }
        }
    }
}