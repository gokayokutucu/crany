using Crany.Common.ModelBinder;
using Microsoft.AspNetCore.Mvc;

namespace Crany.Common.Attributes;

//[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Delegate)]
public class FromAuthorizationHeaderAttribute() : ModelBinderAttribute(typeof(TokenModelBinder));