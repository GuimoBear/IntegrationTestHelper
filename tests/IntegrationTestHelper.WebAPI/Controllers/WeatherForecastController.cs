using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntegrationTestHelper.WebAPI.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Route("pagamentos/forma-pagamento/v{version:apiVersion}/pix")]
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

        [HttpGet("teste")]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost("teste/{route-parameter-with-name}/")]
        public Task<IActionResult> Post([FromBody] WeatherForecast weatherForecast,
                               [FromRoute(Name = "route-parameter-with-name")] string routeWithCustomName,
                               [FromRoute] string routeWithoutCustomName,
                               [FromHeader(Name = "header-parameter-with-name")] string headerWithCustomName, 
                               [FromHeader] string headerParameterWithoutCustomName, 
                               [FromQuery(Name = "query-parameter-with-name")] string queryParameterWithCustomName,
                               [FromQuery] string queryParameterWithoutCustomName, 
                               [FromQuery(Name = "query-array-parameter")] string[] queryArrayParameter)
        {
            return Task.FromResult<IActionResult>(Ok());
        }
    }
}
