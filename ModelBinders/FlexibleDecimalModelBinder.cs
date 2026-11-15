using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SmartStock.ModelBinders;

public sealed class FlexibleDecimalModelBinder : IModelBinder
{
    private const NumberStyles Styles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueResult);
        var value = valueResult.FirstValue;
        if (string.IsNullOrWhiteSpace(value))
        {
            return Task.CompletedTask;
        }

        if (decimal.TryParse(value, Styles, CultureInfo.InvariantCulture, out var amount) ||
            decimal.TryParse(value, Styles, CultureInfo.CurrentCulture, out amount))
        {
            bindingContext.Result = ModelBindingResult.Success(amount);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Enter a valid amount.");
        }

        return Task.CompletedTask;
    }
}
