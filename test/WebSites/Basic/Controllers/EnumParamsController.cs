using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Basic.Controllers
{
    [Route("/enumparams")]
    [Produces("application/json")]
    public class EnumParamsController
    {
        /// <summary>
        /// Retrieve all products matching the input status
        /// </summary>
        /// <param name="status">Status filter for products</param>
        /// <returns>List of matching products</returns>
        [HttpGet]
        public List<Product> ProductsByStatus(ProductStatus status = ProductStatus.All) =>
            new List<Product>();
    }
}
