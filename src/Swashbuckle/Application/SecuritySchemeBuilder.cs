using System;
using System.Collections.Generic;
using System.Linq;
using Swashbuckle.Swagger;

namespace Swashbuckle.Application
{
    public abstract class SecuritySchemeBuilder
    {
        internal abstract SecurityScheme Build();
    }

    public class BasicAuthSchemeBuilder : SecuritySchemeBuilder
    {
        private string _description;

        public BasicAuthSchemeBuilder Description(string description)
        {
            _description = description;
            return this;
        }

        internal override SecurityScheme Build()
        {
            return new SecurityScheme
            {
                type = "basic",
                description = _description
            };
        }
    }

    public class ApiKeySchemeBuilder : SecuritySchemeBuilder
    {
        private string _description;
        private string _name;
        private string _in;

        public ApiKeySchemeBuilder Description(string description)
        {
            _description = description;
            return this;
        }

        public ApiKeySchemeBuilder Name(string name)
        {
            _name = name;
            return this;
        }

        public ApiKeySchemeBuilder In(string @in)
        {
            _in = @in;
            return this;
        }

        internal override SecurityScheme Build()
        {
            return new SecurityScheme
            {
                type = "apiKey",
                description = _description,
                name = _name,
                @in = _in
            };
        }
    }

    public class OAuth2SchemeBuilder : SecuritySchemeBuilder
    {
        private string _description;
        private string _flow;
        private string _authorizationUrl;
        private string _tokenUrl;
        private IDictionary<string, string> _scopes = new Dictionary<string, string>();

        public OAuth2SchemeBuilder Description(string description)
        {
            _description = description;
            return this;
        }

        public OAuth2SchemeBuilder Flow(string flow)
        {
            _flow = flow;
            return this;
        }

        public OAuth2SchemeBuilder AuthorizationUrl(string authorizationUrl)
        {
            _authorizationUrl = authorizationUrl;
            return this;
        }

        public OAuth2SchemeBuilder TokenUrl(string tokenUrl)
        {
            _tokenUrl = tokenUrl;
            return this;
        }

        public OAuth2SchemeBuilder Scopes(Action<IDictionary<string, string>> configure)
        {
            configure(_scopes);
            return this;
        }

        internal override SecurityScheme Build()
        {
            // TODO: Validate required fields for given flow

            return new SecurityScheme
            {
                type = "oauth2",
                flow = _flow,
                authorizationUrl = _authorizationUrl,
                tokenUrl = _tokenUrl,
                scopes = _scopes,
                description = _description,
            };
        }
    }
}