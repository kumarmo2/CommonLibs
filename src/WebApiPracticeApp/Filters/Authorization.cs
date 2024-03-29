using Microsoft.AspNetCore.Mvc.Filters;
using CommonLibs.RedisRateLimiter;
using Microsoft.AspNetCore.Mvc;

namespace CommonLibs.WebApiPracticeApp.Filters;


public class AuthorizationFilter : IAsyncAuthorizationFilter
{
  private readonly IApiThrottler _apiThrottler;
  public AuthorizationFilter(IApiThrottler apiThrottler)
  {
    _apiThrottler = apiThrottler;
  }

  public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
  {
    var attributes = context.ActionDescriptor.EndpointMetadata.OfType<ApiRateLimitAttribute>();
    if(attributes is null || !attributes.Any())
    {
      return;
    }
    context.HttpContext.Request.Headers.TryGetValue("userid", out var values);
    if(!values.Any())
    {
      throw new Exception("for authorization, userId must be present in the request headers");
    }
    var userId = values[0];

    var rules = attributes.Select(attribute => {
        return new ApiRateLimitConfig
        {
          MaxRequests = attribute.MaxRequests,
          Unit = attribute.Unit,
          Window = attribute.Window,
          UniqueClientIdentifier = userId,
          Path = context.HttpContext.Request.Path,
        }; });

    var throttleResponse = await _apiThrottler.ShouldThrottle(rules);

    if(throttleResponse is null)
    {
      Console.WriteLine("some error");
      return;
    }

    if(throttleResponse.ShouldThrottle)
    {
      context.HttpContext.Response.StatusCode = 429;
      context.Result = new ObjectResult(null){ StatusCode = 429};
      return;
    }
  }
}
