using System;
using System.Collections.Generic;
using System.Linq;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SchemaIdManager
    {
        private readonly Func<Type, string> _schemaIdSelector;
        private readonly IDictionary<Type, string> _schemaIdMap;

        public SchemaIdManager(Func<Type, string> schemaIdSelector)
        {
            _schemaIdSelector = schemaIdSelector;
            _schemaIdMap = new Dictionary<Type, string>();
        }

        public string IdFor(Type type)
        {
            string schemaId;
            if (!_schemaIdMap.TryGetValue(type, out schemaId))
            {
                schemaId = _schemaIdSelector(type);

                // Raise an exception if another type with same schemaId
                if (_schemaIdMap.Any(entry => entry.Value == schemaId))
                    throw new InvalidOperationException(string.Format(
                        "Conflicting schemaIds: Identical schemaIds detected for types {0} and {1}. " +
                        "See config settings - \"CustomSchemaIds\" for a workaround",
                        type.FullName, _schemaIdMap.First(entry => entry.Value == schemaId).Key));

                _schemaIdMap.Add(type, schemaId);
            }

            return schemaId;
        }
    }
}
