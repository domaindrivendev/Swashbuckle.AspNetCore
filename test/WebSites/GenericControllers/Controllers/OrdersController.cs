using Microsoft.AspNetCore.Mvc;

namespace GenericControllers.Controllers
{
    [Route("orders")]
    public class OrdersController : GenericResourceController<Order>
    { }

    public class Order
    {
        public int Id { get; set; }
        public decimal Subtotal { get; set; }
    }

}
