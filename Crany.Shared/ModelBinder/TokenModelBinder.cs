using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Crany.Shared.ModelBinder;

public class TokenModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (!bindingContext.HttpContext.Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        var token = authorizationHeader.ToString().Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase).Trim();
        bindingContext.Result = ModelBindingResult.Success(token);
        return Task.CompletedTask;
    }
}