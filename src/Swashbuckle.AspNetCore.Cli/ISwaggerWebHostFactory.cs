using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Swashbuckle.AspNetCore.Cli
{
    public interface ISwaggerWebHostFactory
    {
        IWebHost BuildWebHost(); 
    }
}
