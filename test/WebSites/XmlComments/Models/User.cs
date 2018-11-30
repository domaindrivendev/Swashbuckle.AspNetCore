namespace XmlComments.Models
{
    /// <summary>
    /// User info
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique id for the user
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Unique username for the user
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Current status for the user
        /// </summary>
        public UserStatus Status { get; set; }
    }
}
