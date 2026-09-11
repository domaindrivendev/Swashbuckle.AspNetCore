using System.ComponentModel.DataAnnotations;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test.Fixtures;

public class FakeFormModelWithNestedType
{
    public FakeFormNestedType Inner { get; set; }

    [Required]
    public string Other { get; set; }
}

public class FakeFormNestedType
{
    [Required]
    public string Prop { get; set; }
}
