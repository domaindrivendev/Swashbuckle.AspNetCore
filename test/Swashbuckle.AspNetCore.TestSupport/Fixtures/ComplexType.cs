using System.ComponentModel;

namespace Swashbuckle.AspNetCore.TestSupport;

public class ComplexType
{
    public bool Property1 { get; set; }

    public int Property2 { get; set; }

    [DefaultValue(DayOfWeek.Thursday)]
    public DayOfWeek Property3 { get; set; }
}
