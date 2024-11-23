using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace NSwagClientExample.Controllers
{
#if NET7_0_OR_GREATER
    [ApiController]
    [Route("[controller]")]
    public class SystemTextJsonAnimalsController : ControllerBase
    {
        [HttpPost]
        public void CreateAnimal([Required]SystemTextJsonAnimal animal)
        {
            throw new NotImplementedException();
        }
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "animalType")]
    [JsonDerivedType(typeof(SystemTextJsonCat), "Cat")]
    [JsonDerivedType(typeof(SystemTextJsonDog), "Dog")]
    public class SystemTextJsonAnimal
    {
        public string AnimalType { get; set; }
    }

    public class SystemTextJsonCat : SystemTextJsonAnimal
    {
        public string CatSpecificProperty { get; set; }
    }

    public class SystemTextJsonDog : SystemTextJsonAnimal
    {
        public string DogSpecificProperty { get; set; }
    }
#endif
}
