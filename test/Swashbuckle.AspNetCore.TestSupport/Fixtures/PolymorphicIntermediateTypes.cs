namespace Swashbuckle.AspNetCore.TestSupport;

public abstract class ModelOfA
{
    public string PropertyOfA { get; set; }
}

public class ModelOfB : ModelOfA
{
    public string PropertyOfB { get; set; }
}

public class ModelOfC : ModelOfB
{
    public string PropertyOfC { get; set; }
}
