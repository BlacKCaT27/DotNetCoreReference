using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Bcss.Reference.Web.Validators
{
    public class TemperatureScaleAttribute : ValidationAttribute
    {
        private static readonly IEnumerable<string> ValidScales = new List<string>
        {
            "k",
            "kelvin",
            "c",
            "celsius",
            "f",
            "fahrenheit"
        };

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var scale = value as string;
            if (scale is null)
            {
                return new ValidationResult("Provided scale is not a valid string.");
            }

            var isValid = ValidateScale(scale);

            if (!isValid)
            {
                return new ValidationResult("Provided scale is not a valid scale. Valid values are: 'k', 'kelvin', 'f', 'fahrenheit', 'c', 'celsius', case insensitive.");
            }

            return ValidationResult.Success;
        }

        public static bool ValidateScale(string scale)
        {
            var scaleLower = scale.ToLower();

            return ValidScales.Contains(scaleLower);
        }
    }
}