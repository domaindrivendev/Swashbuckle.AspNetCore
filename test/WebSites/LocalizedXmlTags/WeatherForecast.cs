using System;

namespace LocalizedXmlTags
{
    /// <summary>
    /// WeatherForecast (eng)
    /// </summary>
    /// <summary xml:lang="ru">
    /// WeatherForecast (на русском)
    /// </summary>
    /// <remarks>
    /// WeatherForecast description (eng)
    /// </remarks>
    /// <remarks xml:lang="ru">
    /// Описание WeatherForecast (на русском)
    /// </remarks>
    public class WeatherForecast
    {
        /// <summary>
        /// Date (eng)
        /// </summary>
        /// <summary xml:lang="ru">
        /// Дата (на русском)
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// TemperatureC (eng)
        /// </summary>
        /// <summary xml:lang="ru">
        /// Температура по Цельсию (на русском)
        /// </summary>
        public int TemperatureC { get; set; }

        /// <summary>
        /// TemperatureF (eng)
        /// </summary>
        /// <summary xml:lang="ru">
        /// Температура по Фаренгейту (на русском)
        /// </summary>
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        /// <summary>
        /// Summary (eng)
        /// </summary>
        /// <summary xml:lang="ru">
        /// Summary (на русском)
        /// </summary>
        /// <example>Some summary (eng)</example>
        /// <example xml:lang="ru">Некоторый summary (на русском)</example>
        public string Summary { get; set; }
    }
}
