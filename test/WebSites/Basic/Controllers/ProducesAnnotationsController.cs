using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Basic.Controllers
{
    [Route("/widgets")]
    [Produces("application/json")]
    public class ProducesAnnotationsController : Controller
    {
        public static List<Widget> Widgets = new List<Widget>
        {
            new Widget { Id = 1, Name = "Awesome Widget", Color = "Red" },
            new Widget { Id = 2, Name = "Gnarly Widget", Color = "Green" },
            new Widget { Id = 3, Name = "Rad Widget", Color = "Blue" }
        };

        [HttpGet()]
        public IActionResult Get()
        {
            return Ok(Widgets);
        }

        [HttpGet("{id:int}")]
        [Produces("application/json", "application/xml")]
        public IActionResult Get(int id)
        {
            return Ok(Widgets.Single(w => w.Id == id));
        }

        [HttpGet("picture")]
        [Produces("image/png")]
        public IActionResult Picture()
        {
            byte[] bytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAB4AAAAeCAMAAAAM7l6QAAAAYFBMVEUAAABUfwBUfwBUfwBUfwBUfwBUfwBUfwBUfwBUfwBUfwBUfwBUfwBUfwBUfwB0lzB/n0BfhxBpjyC0x4////+qv4CJp1D09++ft3C/z5/K16/U379UfwDf58/q79+Ur2D2RCk9AAAAHXRSTlMAEEAwn9//z3Agv4/vYID/////////////////UMeji1kAAAD5SURBVHgBlZKFgYBADATRxe0Ibv1X+bK4vQzOnCbR/oNuGIb54gzLBnFc7y59HLAuDQKcCaPjnA5uxLumveK+jbxgLKsCSdIMRKWKT1un5pqRi6QghUi5T2+ASFWvvRtpweV/a2vRHcjxvf/S9rsOvvYMok56APG3qXMpkAAqQ5ZgWFe56k6GrJZayYhWkrSS4qRzqZJSSjW2yCtVSzWdNCbJsTFKtu0MJHtaub/n40nHTMhVbyN9F5YHUkmjQNJBGr7Y2h7VqdpTMiZ8iai9EI842kyEJ8KtpK33Ynkpp7DXDvSX+R2OvKPHhwY213xGjywG0A/2WT8BXsgXQDtZUQsAAAAASUVORK5CYII=");
            return File(bytes, "image/png");
        }
    }

    public class Widget
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
    }
}
