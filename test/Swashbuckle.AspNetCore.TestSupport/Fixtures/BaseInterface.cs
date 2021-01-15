namespace Swashbuckle.AspNetCore.TestSupport
{
    public interface IBaseInterface
    {
        public string BaseProperty { get; set; }
    }

    public interface ISubInterface1 : IBaseInterface
    {
        public int Property1 { get; set; }
    }

    public interface ISubInterface2 : IBaseInterface
    {
        public int Property2 { get; set; }
    }

    public interface IMultiSubInterface : ISubInterface1, ISubInterface2
    {
        public int Property3 { get; set; }
    }
}
