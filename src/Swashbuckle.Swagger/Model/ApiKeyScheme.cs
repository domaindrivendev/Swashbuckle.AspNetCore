namespace Swashbuckle.Swagger.Model
{
    public class ApiKeyScheme : SecurityScheme
    {
        /// <summary>
        /// The name of the header or query parameter to be used.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The location of the API key. Valid values are "query" or "header".
        /// </summary>
        public string In { get; set; }

        public ApiKeyScheme()
        {
            Type = "apiKey";
        }
    }
}
