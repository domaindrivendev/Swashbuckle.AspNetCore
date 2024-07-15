using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test.Fixtures
{
    internal class FakeGenericResourceController<T>
        where T : class
    {
        public virtual int DifferentMethodsSignatures([FromBody, Required] T t, CancellationToken cancellationToken) => 1;
        public virtual int DifferentMethodsSignatures([FromBody, Required] string key, T t) => 1;
        public virtual int DifferentMethodsSignatures([FromBody, Required] T[] ts) => 1;
        public virtual int DifferentMethodsSignatures([FromBody, Required] string[] strings) => 1;

        public virtual int DifferentMethodsSignatures([FromBody, Required] List<T> ts) => 1;
        public virtual int DifferentMethodsSignatures([FromBody, Required] List<string> strings) => 1;

        public virtual int DifferentMethodsSignatures([FromBody, Required] Dictionary<int, T> ts) => 1;

        public virtual int SimilarMethodsSignatures([FromBody, Required] List<T> ts) => 1;
        public virtual int SimilarMethodsSignatures([FromBody, Required] IEnumerable<T> ts) => 1;
    }
    internal class NonGenericResourceController : FakeGenericResourceController<XmlAnnotatedRecord>
    {

    }
}
