using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace NSwagClientExample.Controllers;

[ApiController]
[Route("[controller]")]
public class SystemTextJsonCustomDiscriminatorAnimalsController : ControllerBase
{
    [HttpPost]
    [Produces("application/json")]
    public void CreateAnimal([Required] SystemTextJsonCustomDiscriminatorAnimal animal)
    {
        throw new NotImplementedException();
    }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "animalType")]
[JsonDerivedType(typeof(SystemTextJsonCustomDiscriminatorCat), "Cat")]
[JsonDerivedType(typeof(SystemTextJsonCustomDiscriminatorDog), "Dog")]
public class SystemTextJsonCustomDiscriminatorAnimal
{
    public string AnimalType { get; set; }
}

public class SystemTextJsonCustomDiscriminatorCat : SystemTextJsonCustomDiscriminatorAnimal
{
    public string CatSpecificProperty { get; set; }
}

public class SystemTextJsonCustomDiscriminatorDog : SystemTextJsonCustomDiscriminatorAnimal
{
    public string DogSpecificProperty { get; set; }
}
