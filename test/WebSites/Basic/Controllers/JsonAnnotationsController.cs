using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Basic.Controllers
{
    [Produces("application/json")]
    public class JsonAnnotationsController
    {
        [HttpGet("/promotions")]
        public IEnumerable<Promotion> GetPromotions()
        {
            return new[]
            {
                new Promotion { Code = "A", DiscountType = DiscountType.Amount, Discount = 30 },
                new Promotion { Code = "B", DiscountType = DiscountType.Percentage, Discount = 10 }
            };
        }
    }

    public class Promotion
    {
        [JsonPropertyName("promo-code")]
        public string Code { get; set; }

        public DiscountType DiscountType { get; set; }

        [JsonIgnore]
        public int Discount { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DiscountType
    {
        Percentage,
        Amount
    }
}