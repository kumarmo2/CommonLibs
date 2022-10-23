using CommonLibs.RedisRateLimiter;
using Microsoft.AspNetCore.Mvc;
using WebApiPracticeApp;
using CommonLibs.WebApiPracticeApp.Filters;

namespace CommonLibs.WebApiPracticeApp.Controllers;

[ApiController]
[Route("[controller]")]
[ServiceFilter(typeof(AuthorizationFilter))]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [ApiRateLimitAttribute(Window = 1, Unit = TimeUnit.M, MaxRequests = 5)]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
