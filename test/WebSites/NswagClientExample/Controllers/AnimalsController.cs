using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace NSwagClientExample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AnimalsController : ControllerBase
    {
        [HttpPost]
        public void CreateAnimal([Required]Animal animal)
        {
            throw new NotImplementedException();
        }
    }

    [SwaggerDiscriminator("animalType")]
    [SwaggerSubType(typeof(Cat), DiscriminatorValue = "Cat")]
    [SwaggerSubType(typeof(Dog), DiscriminatorValue = "Dog")]
    public class Animal
    {
        public AnimalType AnimalType { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AnimalType
    {
        Cat,
        Dog
    }

    public class Cat : Animal
    {
        public string CatSpecificProperty { get; set; }
    }

    public class Dog : Animal
    {
        public string DogSpecificProperty { get; set; }
    }
}
