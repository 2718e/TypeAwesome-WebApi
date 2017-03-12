using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeAwesomeWebApi;
using Utils;

namespace Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            var commandParser = new CommandLineParser(args);
            var outPath = commandParser.GetOptionArgument("--out-path", Path.Combine(".", "typeawesomeresults.ts"));
            var thingToExport = Assembly.Load("ServerMethods");
            var config = new TypescriptMakerConfig
            {
                ControllerAssemblies = new Assembly[] { thingToExport }
            };
            var tsBuilder = new TypescriptMaker(config);
            var result = tsBuilder.MakeTypescript();
            File.WriteAllText(outPath, result);
        }
    }
}
