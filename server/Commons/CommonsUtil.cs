using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace GpEnerSaf.Commons
{
    public class CommonsUtil
    {
        public static JObject Convert(string property, IEnumerable<dynamic> rows)
        {
            string json = "{ " + property + " : " + JsonConvert.SerializeObject(rows, Formatting.Indented) + "}";
            return JObject.Parse(json);
        }
    }
}
