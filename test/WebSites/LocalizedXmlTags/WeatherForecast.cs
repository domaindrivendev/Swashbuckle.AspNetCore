using System;

namespace LocalizedXmlTags
{
    /// <summary>
    /// WeatherForecast (eng)
    /// </summary>
    /// <summary xml:lang="ru">
    /// WeatherForecast (�� �������)
    /// </summary>
    /// <remarks>
    /// WeatherForecast description (eng)
    /// </remarks>
    /// <remarks xml:lang="ru">
    /// �������� WeatherForecast (�� �������)
    /// </remarks>
    public class WeatherForecast
    {
        /// <summary>
        /// Date (eng)
        /// </summary>
        /// <summary xml:lang="ru">
        /// ���� (�� �������)
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// TemperatureC (eng)
        /// </summary>
        /// <summary xml:lang="ru">
        /// ����������� �� ������� (�� �������)
        /// </summary>
        public int TemperatureC { get; set; }

        /// <summary>
        /// TemperatureF (eng)
        /// </summary>
        /// <summary xml:lang="ru">
        /// ����������� �� ���������� (�� �������)
        /// </summary>
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        /// <summary>
        /// Summary (eng)
        /// </summary>
        /// <summary xml:lang="ru">
        /// Summary (�� �������)
        /// </summary>
        /// <example>Some summary (eng)</example>
        /// <example xml:lang="ru">��������� summary (�� �������)</example>
        public string Summary { get; set; }
    }
}
