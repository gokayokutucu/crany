using Crany.Web.Api.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Crany.Web.Api.Controllers;

public class BaseController : ControllerBase
{
    /// <summary>
    /// Public User Identifier
    /// </summary>
    protected string? PublicUserId => User.Identity?.PublicId();
}