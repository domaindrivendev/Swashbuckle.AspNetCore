using System;

namespace Basic
{
    public class GenericType<T>
    {
        public T Property1 { get; set; }
    }

    public class GenericType<T,K>
    {
        public T Property1 { get; set; }

        public K Property2 { get; set; }
    }
}
