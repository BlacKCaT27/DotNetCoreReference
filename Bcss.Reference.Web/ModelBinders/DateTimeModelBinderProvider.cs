using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Bcss.Reference.Web.ModelBinders
{
    /// <summary>
    /// The default model binding for date-time does not handle the UTC "Z" timezone flag properly.
    /// This Provider, and the UtcAwareDateTimeModelBinder, are modified versions of the fix outlined in
    /// this SO answer: https://stackoverflow.com/questions/10293440/how-to-make-asp-net-mvc-model-binder-treat-incoming-date-as-utc/46308876#46308876
    /// </summary>
    public class DateTimeModelBinderProvider : IModelBinderProvider
    {
        /// <inheritdoc />
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var modelType = context.Metadata.UnderlyingOrModelType;
            if (modelType == typeof(DateTime))
            {
                return new UtcAwareDateTimeModelBinder();
            }

            return null;
        }
    }
}