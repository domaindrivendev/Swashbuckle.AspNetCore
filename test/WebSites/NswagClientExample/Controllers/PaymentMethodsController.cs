#if NET7_0_OR_GREATER
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace NSwagClientExample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentMethodsController : ControllerBase
    {
        [HttpPost]
        public void AddPaymentMethod([Required]PaymentMethod paymentMethod)
        {
            throw new NotImplementedException();
        }
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "paymentMethod")]
    [JsonDerivedType(typeof(CreditCard), "CreditCard")]
    [JsonDerivedType(typeof(MobileWallet), "MobileWallet")]
    public class PaymentMethod
    {
    }

    public class CreditCard : PaymentMethod
    {
        public string CardNumber { get; set; }
    }

    public class MobileWallet : PaymentMethod
    {
        public string WalletId { get; set; }
    }
}
#endif
