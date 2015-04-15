using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SampleApi.Controllers
{
    [Produces("application/json")]
    public class JsonAnnotationsController
    {
        [HttpGet("/promotions")]
        public IEnumerable<Promotion> GetPromotions()
        {
            throw new NotImplementedException();
        }
    }

    public class Promotion
    {
        [JsonProperty("promo-code")]
        public string Code { get; set; }

        public DiscountType DiscountType { get; set; }

        [JsonIgnore]
        public int Discount { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum DiscountType
    {
        Percentage,
        Amount
    }
}