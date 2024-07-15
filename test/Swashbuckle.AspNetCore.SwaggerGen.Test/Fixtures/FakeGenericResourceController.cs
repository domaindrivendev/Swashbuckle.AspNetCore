using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test.Fixtures
{
    internal class FakeGenericResourceController<T>
        where T : class
    {
        public virtual int Create([FromBody, Required] T t, CancellationToken cancellationToken) => 1;
        public virtual int Create([FromBody, Required] string key, T t) => 1;
        public virtual int Create([FromBody, Required] T[] ts) => 1;
        public virtual int Create([FromBody, Required] string[] strings) => 1;

        public virtual int Create([FromBody, Required] List<T> ts) => 1;
        public virtual int Create([FromBody, Required] List<string> strings) => 1;

        public virtual int Create([FromBody, Required] Dictionary<int, T> ts) => 1;
    }
    internal class NonGenericResourceController : FakeGenericResourceController<XmlAnnotatedRecord>
    {

    }
}
