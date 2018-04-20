namespace Swashbuckle.AspNetCore.Annotations.Test.Fixtures
{
    [SwaggerTag("Tag1", "Description1")]
    [SwaggerTag("Tag2", "Description2")]
    [SwaggerTag("Tag42", "Description42")]
    internal class TaggedController
    {
        public void EmptyAction()
        { }
    }
}
