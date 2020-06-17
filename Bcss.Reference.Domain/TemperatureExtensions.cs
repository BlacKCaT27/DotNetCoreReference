using System;

namespace Bcss.Reference.Domain
{
    public static class TemperatureExtensions
    {
        /*
         * Extension methods are a powerful tool in C#, but they must
         * be used wisely. They're well suited to adding utility to existing
         * classes that you may not be able to alter directly (such as framework
         * classes). This can help a lot with avoiding static utility methods and
         * helper classes which makes the calling code feel more object-oriented and
         * less procedural, reducing complexity in the process. However, they shouldn't
         * be used in all cases. Extension methods should never be used when they would rely
         * on the internal state of a class they're extending (beyond public getters).
         * They also can make classes harder to mock. If you were to add an extension method
         * to, say, `IFoo`:
         *
         * public static void Bar(this IFoo foo){
         *   Console.Writeline(foo);
         * }
         *
         * Note the `this` keyword. That keyword is what marks the signature as an extension method.
         * Extension methods must be static, the first parameter must be an instance of the type being
         * extended preceded by `this`, and they must be contained within a static class.
         *
         * If, during unit testing, you create a mock object, you'll find not all
         * mocking frameworks will capture your extension method.
         *
         * Some of the best candidates for extension methods are to extend the capabilities of a class
         * that you are not responsible for testing.
         *
         * Extension methods are also used heavily by 3rd party libraries (and even
         * the framework itself) to provide registration methods for various features.
         * The `AddControllers()` method in `Startup.cs`, for example, is an extension method
         * on the `IServiceCollection` class. It's very common to write extension methods
         * to expose middleware or even just cleanly register whatever types your library needs
         * with the DI framework.
         */

        public static Kelvin ToKelvin(this Celsius celsius)
        {
            return new Kelvin(celsius.Value + (decimal)273.15);
        }

        public static Fahrenheit ToFahrenheit(this Celsius celsius)
        {
            return new Fahrenheit(celsius.Value * 9 / 5 + 32);
        }

        public static Celsius ToCelsius(this Fahrenheit fahrenheit)
        {
            return new Celsius((fahrenheit.Value - 32) * 5 / 9 );
        }

        public static Kelvin ToKelvin(this Fahrenheit fahrenheit)
        {
            return new Kelvin((fahrenheit.Value + (decimal)459.67) * 5 / 9);
        }

        public static Celsius ToCelsius(this Kelvin kelvin)
        {
            return new Celsius(kelvin.Value - (decimal)273.15);
        }

        public static Fahrenheit ToFahrenheit(this Kelvin kelvin)
        {
            return new Fahrenheit(kelvin.Value * 9 / 5 - (decimal)459.67);
        }

        #region Intentionally breaking encapsulation

        /**
         * The methods below bear some discussion. Here, we're intentionally breaking
         * the encapsulation provided by the `Temperature` class. But, importantly,
         * it's done with explicit intent. There is business value in a `TemperatureC`
         * object knowing what scale it represents (since a flexible API can be accommodating to varying
         * data types), just as there's business value in having explicit temperature scale types
         * (that can each contain their own enforcement rules for what constitutes a valid value).
         *
         * The key take-away here is that rules are rules; and rules are meant to be broken.
         * But they MUST be broken with clear and documented intent, and only when
         * necessary, not simply because it's the path of least resistance. You should always
         * call out in comments when and why you break a rule, and you should try to keep the damage
         * as contained as possible to a single area. Be prepared to back up your decision during code
         * reviews.
         */

        public static Kelvin ToKelvin(this Temperature temperature)
        {
            switch (temperature.Scale)
            {
                case Scale.Celsius:
                    return ((Celsius)temperature).ToKelvin();
                case Scale.Fahrenheit:
                    return ((Fahrenheit)temperature).ToKelvin();
                case Scale.Kelvin:
                    return (Kelvin)temperature;
                case Scale.Unknown:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Celsius ToCelsius(this Temperature temperature)
        {
            switch (temperature.Scale)
            {
                case Scale.Celsius:
                    return (Celsius)temperature;
                case Scale.Fahrenheit:
                    return ((Fahrenheit)temperature).ToCelsius();
                case Scale.Kelvin:
                    return ((Kelvin)temperature).ToCelsius();
                case Scale.Unknown:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Fahrenheit ToFahrenheit(this Temperature temperature)
        {
            switch (temperature.Scale)
            {
                case Scale.Celsius:
                    return ((Celsius)temperature).ToFahrenheit();
                case Scale.Fahrenheit:
                    return (Fahrenheit)temperature;
                case Scale.Kelvin:
                    return ((Kelvin)temperature).ToFahrenheit();
                case Scale.Unknown:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}