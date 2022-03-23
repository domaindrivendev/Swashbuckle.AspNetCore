using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test.Fixtures
{
    public class TestMinimalApiMethod
    {
        public static Task RequestDelegate(long id)
        {
            return Task.FromResult(id);
        }
    }
}
