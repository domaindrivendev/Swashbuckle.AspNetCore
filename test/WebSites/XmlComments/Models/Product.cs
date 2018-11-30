namespace XmlComments.Models
{
    /// <summary>
    /// Product info
    /// </summary>
    public class Product
    {
        /// <summary>
        /// A unique id for the product
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// A description for the product
        /// </summary>
        public string Description { get; set; }
    }
}