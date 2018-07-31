using Microsoft.AspNetCore.Hosting;
using Swashbuckle.AspNetCore.Cli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CliExampleWithFactory
{
    public class MySwaggerWebHostFactory : ISwaggerWebHostFactory
    {
        public IWebHost BuildWebHost()
        {
            return Program.BuildWebHost(new string[0]);
        }
    }
}
