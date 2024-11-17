using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Crany.Shared.Extensions;

public static class ApiDescriptionExtensions
{
    public static bool IsDeprecated(this ApiDescription apiDescription)
    {
        if (apiDescription.ActionDescriptor.EndpointMetadata
                .OfType<ApiVersionAttribute>()
                .FirstOrDefault() is { } apiVersionAttribute)
        {
            return apiVersionAttribute.Versions.Any(v => v.Status == ApiVersionStatus.Deprecated.ToString());
        }

        return false;
    }
}

public enum ApiVersionStatus
{
    Stable,
    Deprecated,
    Beta,
    Experimental
}