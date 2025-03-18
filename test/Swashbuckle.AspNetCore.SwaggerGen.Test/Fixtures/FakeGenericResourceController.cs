using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test.Fixtures;

internal class FakeGenericResourceController<T>
    where T : class
{
    public byte DifferentMethodsSignatures([FromBody, Required] T t, CancellationToken cancellationToken) => default;
    public sbyte DifferentMethodsSignatures([FromBody, Required] string key, T t) => default;
    public short DifferentMethodsSignatures([FromBody, Required] T[] arrayOfTs) => default;
    public ushort DifferentMethodsSignatures([FromBody, Required] string[] arrayOfStrings) => default;
    public int DifferentMethodsSignatures([FromBody, Required] List<T> listOfTs) => default;
    public uint DifferentMethodsSignatures([FromBody, Required] List<string> listOfStrings) => default;
    public long DifferentMethodsSignatures([FromBody, Required] Dictionary<int, T> dictionaryOfTs) => default;
    public ulong DifferentMethodsSignatures([FromBody, Required] IEnumerable<T> iEnumerableOfTs) => default;
}

internal class NonGenericResourceController : FakeGenericResourceController<XmlAnnotatedRecord>;
