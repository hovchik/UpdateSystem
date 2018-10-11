using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgainServer.Models.Models
{
    public interface IStaticpaths
    {
        string StatPath { get; set; }
    }

    public class StaticPath : IStaticpaths
    {
        public string StatPath { get; set; }
    }
}
