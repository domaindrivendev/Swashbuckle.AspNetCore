using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace Basic.Controllers
{
    [Route("/products")]
    [Produces("application/json")]
    public class CrudActionsController
    {
        /// <summary>
        /// Creates a <paramref name="product"/>
        /// </summary>
        /// <remarks>
        /// ## Heading 1
        /// 
        ///     POST /products
        ///     {
        ///         "id": "123",
        ///         "description": "Some product"
        ///     }
        /// 
        /// </remarks>
        /// <param name="product"></param>
        /// <returns></returns>
        [HttpPost]
        public int Create([FromBody, Required]Product product)
        {
            return 1;
        }

        /// <summary>
        /// Searches the collection of products by description key words
        /// </summary>
        /// <param name="keywords">A list of search terms</param>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<Product> Search([FromQuery(Name = "kw")]string keywords)
        {
            return new[]
            {
                new Product { Id = 1, Description = "A product" },
                new Product { Id = 2, Description = "Another product" },
            };
        }

        /// <summary>
        /// Returns a specific product 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public Product GetById(int id)
        {
            return new Product { Id = id, Description = "A product" };
        }

        /// <summary>
        /// Returns a specific product  from a strongly typed key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet("struct/{key}")]
        public Product GetStructById(ProductKey key)
        {
            return new Product { Id = key.Id, Description = "A product" };
        }

        /// <summary>
        /// Updates all properties of a specific product
        /// </summary>
        /// <param name="id"></param>
        /// <param name="product"></param>
        [HttpPut("{id}")]
        public void Update(int id, [FromBody, Required]Product product)
        {
        }

        /// <summary>
        /// Updates some properties of a specific product
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updates"></param>
        [HttpPatch("{id}")]
        public void PartialUpdate(int id, [FromBody, Required]IDictionary<string, object> updates)
        {
        }

        /// <summary>
        /// Deletes a specific product
        /// </summary>
        /// <param name="id"></param>
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }

    public enum ProductStatus
    {
        All = 0,
        [Display(Name = "Out Of Stock")]
        OutOfStock = 1,
        InStock = 2
    }

    /// <summary>
    /// Represents a product
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Uniquely identifies the product
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Describes the product
        /// </summary>
        public string Description { get; set; }

        public ProductStatus Status { get; set; }
    }

    /// <summary>
    /// Represents a strongly typed product identifier.
    /// </summary>
    [ModelBinder(typeof(ProductKeyModelBinder))]
    [JsonConverter(typeof(ProductKeyConverter))]
    public struct ProductKey
    {
        private readonly int id;

        public ProductKey(int id)
        {
            this.id = id;
        }

        public int Id { get { return id; } }

        public sealed class ProductKeyConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(ProductKey);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var rawId = int.Parse(reader.Value.ToString());
                return new ProductKey(rawId);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var id = ((ProductKey)value).id;
                writer.WriteValue(id.ToString(CultureInfo.InvariantCulture));
            }
        }

        public sealed class ProductKeyModelBinder : IModelBinder
        {
            public Task BindModelAsync(ModelBindingContext bindingContext)
            {
                var sid = bindingContext.ValueProvider.GetValue(bindingContext.FieldName).FirstValue;
                bindingContext.Result = ModelBindingResult.Success(new ProductKey(int.Parse(sid, CultureInfo.InvariantCulture)));
                return Task.CompletedTask;
            }
        }
    }
}
