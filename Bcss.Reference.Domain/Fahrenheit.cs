namespace Bcss.Reference.Domain
{
    public class Fahrenheit : Temperature
    {
        private const decimal AbsoluteZeroInFahrenheit = (decimal)-459.67;

        public Fahrenheit(decimal temperature)
        {
            Value = temperature;
            Scale = Scale.Fahrenheit;
            MinValue = AbsoluteZeroInFahrenheit;
            Validate();
        }
    }
}