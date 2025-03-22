namespace Swashbuckle.AspNetCore.SwaggerGen;

public class SwaggerGenOptions
{
    public SwaggerGeneratorOptions SwaggerGeneratorOptions { get; set; } = new();

    public SchemaGeneratorOptions SchemaGeneratorOptions { get; set; } = new();

    // NOTE: Filter instances can be added directly to the options exposed above OR they can be specified in
    // the following lists. In the latter case, they will be instantiated and added when options are injected
    // into their target services. This "deferred instantiation" allows the filters to be created from the
    // DI container, thus supporting contructor injection of services within filters.

    public List<FilterDescriptor> ParameterFilterDescriptors { get; set; } = [];

    public List<FilterDescriptor> RequestBodyFilterDescriptors { get; set; } = [];

    public List<FilterDescriptor> OperationFilterDescriptors { get; set; } = [];

    public List<FilterDescriptor> DocumentFilterDescriptors { get; set; } = [];

    public List<FilterDescriptor> SchemaFilterDescriptors { get; set; } = [];
}
