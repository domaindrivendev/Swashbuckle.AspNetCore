using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Swashbuckle.AspNetCore.SwaggerGen.SchemaValidator
{
    public class SchemaValidationResult
    {
        public List<SchemaValidationError> Errors { get; }

        private SchemaValidationResult(List<SchemaValidationError> errors)
        {
            Errors = errors;
        }

        internal static SchemaValidationResult Create()
            => new SchemaValidationResult(new List<SchemaValidationError>());

        internal void AddError(SchemaValidationError error)
        {
            Errors.Add(error);
        }

        public bool HasErrors() => Errors.Count > 0;
    }

    public class SchemaValidationError
    {
        public string HttpMethod { get; }
        public string Path { get; }
        public string Reason { get; }

        private SchemaValidationError(string httpMethod, string path, string reason)
        {
            HttpMethod = httpMethod;
            Path = path;
            Reason = reason;
        }

        internal static SchemaValidationError Create(
            ApiDescription api,
            Type expectedResponseType,
            Type methodResultType
        )
            => new SchemaValidationError(
                api.HttpMethod,
                api.RelativePath,
                $"It should return: {expectedResponseType} but instead it returns {methodResultType}"
            );
    }
}