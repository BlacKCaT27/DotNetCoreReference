﻿using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Bcss.Reference.Web.ModelBinders
{
    public class UtcAwareDateTimeModelBinder : IModelBinder
    {
        private static readonly string[] DateTimeFormats = { "yyyyMMdd'T'HHmmss.FFFFFFFK", "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK" };

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var stringValue = bindingContext.ValueProvider.GetValue(bindingContext.ModelMetadata.Name).FirstValue;

            if (bindingContext.ModelType == typeof(DateTime?) && string.IsNullOrEmpty(stringValue))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            bindingContext.Result = DateTime.TryParseExact(stringValue, DateTimeFormats,
                CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result)
                ? ModelBindingResult.Success(result)
                : ModelBindingResult.Failed();

            return Task.CompletedTask;
        }
    }
}