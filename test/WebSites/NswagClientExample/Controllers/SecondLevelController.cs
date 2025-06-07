using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace NswagClientExample.Controllers;

[ApiController]
[Route("[controller]")]
public class SecondLevelController : ControllerBase
{
    [HttpPost]
    [Produces("application/json")]
    public int Create([FromBody] BaseType input)
    {
        throw new NotImplementedException();
    }
}

[SwaggerDiscriminator("discriminator")]
[SwaggerSubType(typeof(SubSubType), DiscriminatorValue = nameof(SubSubType))]
public abstract class BaseType
{
    public string Property { get; set; }
}

public abstract class SubType : BaseType;

public class SubSubType : SubType
{
    public string Property2 { get; set; }
}
