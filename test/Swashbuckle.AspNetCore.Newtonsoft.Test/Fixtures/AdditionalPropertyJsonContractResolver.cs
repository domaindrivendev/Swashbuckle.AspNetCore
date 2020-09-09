using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test
{
    public class AdditionalPropertyJsonContractResolver : DefaultContractResolver
    {
        public const string AdditionalPropertyName = "AdditionalPropertyFromContractResolver";

        private sealed class ConstantValueProvider : IValueProvider
        {
            private readonly object _value;
            public ConstantValueProvider(object value) => _value = value;
            public void SetValue(object target, object value) => throw new NotImplementedException();
            public object GetValue(object target) => _value;
        }

        /// <inheritdoc />
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var result = base.CreateProperties(type, memberSerialization);

            result.Add(new JsonProperty
            {
                PropertyName = AdditionalPropertyName,
                Readable = true,
                Writable = false,
                ValueProvider = new ConstantValueProvider("MyValue"),
                PropertyType = typeof(string),
            });

            return result;
        }
    }
}