using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LocalizedXmlTags.Controllers
{
    /// <summary>
    /// WeatherForecastController
    /// </summary>
    /// <summary xml:lang="ru">
    /// Контроллер погоды (на русском)
    /// </summary>
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

        /// <summary>
        /// Get info (eng)
        /// </summary>
        /// <summary xml:lang="ru">
        /// Получить информацию (на русском)
        /// </summary>
        /// <param name="parameter">Parameter (eng)</param>
        /// <param name="parameter" xml:lang="ru">Параметр (на русском)</param>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<WeatherForecast> Get(int parameter)
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
    }
}
