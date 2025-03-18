namespace Swashbuckle.AspNetCore.TestSupport.Fixtures
{
    public class FakeControllerWithActionWithParamDefaults
    {
        public void ActionWithEnumParamDefaultValue(IntEnum firstParam = IntEnum.Value4) { }
    }
}
