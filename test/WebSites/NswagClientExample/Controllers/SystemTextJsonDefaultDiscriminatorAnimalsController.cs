using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace NSwagClientExample.Controllers;

[ApiController]
[Route("[controller]")]
public class SystemTextJsonDefaultDiscriminatorAnimalsController : ControllerBase
{
    [HttpPost]
    [Produces("application/json")]
    public void CreateAnimal([Required] SystemTextJsonDefaultDiscriminatorAnimal animal)
    {
        throw new NotImplementedException();
    }
}

[JsonPolymorphic()]
[JsonDerivedType(typeof(SystemTextJsonDefaultDiscriminatorCat), "Cat")]
[JsonDerivedType(typeof(SystemTextJsonDefaultDiscriminatorDog), "Dog")]
public class SystemTextJsonDefaultDiscriminatorAnimal
{
    public string AnimalType { get; set; }
}

public class SystemTextJsonDefaultDiscriminatorCat : SystemTextJsonDefaultDiscriminatorAnimal
{
    public string CatSpecificProperty { get; set; }
}

public class SystemTextJsonDefaultDiscriminatorDog : SystemTextJsonDefaultDiscriminatorAnimal
{
    public string DogSpecificProperty { get; set; }
}
