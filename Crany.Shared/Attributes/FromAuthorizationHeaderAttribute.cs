using Crany.Shared.ModelBinder;
using Microsoft.AspNetCore.Mvc;

namespace Crany.Shared.Attributes;

//[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Delegate)]
public class FromAuthorizationHeaderAttribute() : ModelBinderAttribute(typeof(TokenModelBinder));