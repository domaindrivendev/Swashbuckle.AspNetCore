namespace FunctionApp.Models
{
    /// <summary>
    /// Greetings response
    /// </summary>
    public class GreetingsResponseModel
    {
        /// <summary>
        /// Firstname
        /// </summary>
        public string Firstname { get; set; }

        /// <summary>
        /// Lastname
        /// </summary>
        public string  Lastname { get; set; }

        /// <summary>
        /// Greetings message
        /// </summary>
        public string  Message { get; set; }
    }
}