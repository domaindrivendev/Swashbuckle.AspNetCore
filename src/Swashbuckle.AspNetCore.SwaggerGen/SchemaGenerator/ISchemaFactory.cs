using System;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface ISchemaFactory
    {
        OpenApiSchema CreateSchema(SchemaFactoryContext context);
    }

    public class DelegatedSchemaFactory : ISchemaFactory
    {
        private readonly Func<SchemaFactoryContext, OpenApiSchema> _schemaFactory;

        public DelegatedSchemaFactory(Func<SchemaFactoryContext, OpenApiSchema> schemaFactory)
        {
            _schemaFactory = schemaFactory;
        }

        public OpenApiSchema CreateSchema(SchemaFactoryContext context)
        {
            return _schemaFactory.Invoke(context);
        }
    }

    public class SchemaFactoryContext
    {
        public Type Type { get; }

        public DataContract DataContract { get; }

        public ISchemaGenerator SchemaGenerator { get; }

        public SchemaRepository SchemaRepository { get; }

        public SchemaFactoryContext(Type type, DataContract dataContract, ISchemaGenerator schemaGenerator, SchemaRepository schemaRepository)
        {
            Type = type;
            DataContract = dataContract;
            SchemaGenerator = schemaGenerator;
            SchemaRepository = schemaRepository;
        }
    }
}