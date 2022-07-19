using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class FakeAuthenticationSchemeProvider : IAuthenticationSchemeProvider
    {
        private readonly IEnumerable<AuthenticationScheme> _authenticationSchemes;

        public FakeAuthenticationSchemeProvider(IEnumerable<AuthenticationScheme> authenticationSchemes)
        {
            _authenticationSchemes = authenticationSchemes;
        }

        public void AddScheme(AuthenticationScheme scheme)
            => throw new NotImplementedException();
        public Task<IEnumerable<AuthenticationScheme>> GetAllSchemesAsync()
            => Task.FromResult(_authenticationSchemes);

        public Task<AuthenticationScheme> GetDefaultAuthenticateSchemeAsync()
            => Task.FromResult(_authenticationSchemes.First());

        public Task<AuthenticationScheme> GetDefaultChallengeSchemeAsync()
            => Task.FromResult(_authenticationSchemes.First());

        public Task<AuthenticationScheme> GetDefaultForbidSchemeAsync()
            => Task.FromResult(_authenticationSchemes.First());

        public Task<AuthenticationScheme> GetDefaultSignInSchemeAsync()
            => Task.FromResult(_authenticationSchemes.First());

        public Task<AuthenticationScheme> GetDefaultSignOutSchemeAsync()
            => Task.FromResult(_authenticationSchemes.First());

        public Task<IEnumerable<AuthenticationScheme>> GetRequestHandlerSchemesAsync()
            => throw new NotImplementedException();

        public Task<AuthenticationScheme> GetSchemeAsync(string name)
            => Task.FromResult(_authenticationSchemes.First());

        public void RemoveScheme(string name)
            => throw new NotImplementedException();
    }
}