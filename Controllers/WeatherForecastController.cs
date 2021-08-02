using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JWTAuthentication.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
        [Authorize]
        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            // Microsoft.AspNetCore.Http.HttpContext context
            //Microsoft.AspNetCore.Http.HttpContext context
            //Console.WriteLine(context.Request.Headers.ToString());
            Microsoft.Extensions.Primitives.StringValues outt = "";
            HttpContext.Request.Headers.TryGetValue("Authorization", out outt);
            string token = outt.ToString().Split(" ")[1];
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();

            //var handler = new JwtSecurityTokenHandler();
            //var jwtSecurityToken = handler.ReadJwtToken(token);
            //Console.WriteLine(jwtSecurityToken.Claims.ToList()[0]);
            //Console.WriteLine(jwtSecurityToken.Claims.ToList()[1]);
        }
    }
}
