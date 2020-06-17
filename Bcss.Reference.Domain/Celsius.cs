namespace Bcss.Reference.Domain
{
    public class Celsius : Temperature
    {
        private const decimal AbsoluteZeroInCelsius = (decimal)-273.15;

        public Celsius(decimal temperature)
        {
            Value = temperature;
            Scale = Scale.Celsius;
            MinValue = AbsoluteZeroInCelsius;
            Validate();
        }
    }
}