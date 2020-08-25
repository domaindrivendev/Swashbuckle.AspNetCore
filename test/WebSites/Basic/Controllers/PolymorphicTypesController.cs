using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Basic.Controllers
{
    [Route("/shapes")]
    public class PolymorphicTypesController
    {
        [HttpPost]
        public int CreateShape([FromBody]Shape shape)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Shape
    {
        public string Name { get; set; } 
    }

    public class Rectangle : Shape
    {
        public int Height { get; set; }

        public int Width { get; set; }
    }

    public class Circle : Shape
    {
        public int Radius { get; set; }
    }
}
