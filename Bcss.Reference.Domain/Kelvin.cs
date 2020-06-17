namespace Bcss.Reference.Domain
{
    public class Kelvin : Temperature
    {
        private const decimal AbsoluteZeroInKelvin = 0;

        public Kelvin(decimal temperature)
        {
            Value = temperature;
            Scale = Scale.Kelvin;
            MinValue = AbsoluteZeroInKelvin;
            Validate();
        }
    }
}