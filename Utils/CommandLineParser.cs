using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class CommandLineParser
    {

        private List<string> args;

        public CommandLineParser(string[] commandArgs)
        {
            this.args = commandArgs.ToList();
        }

        public bool HasOptionArgument(string arg)
        {
            var argIndex = args.IndexOf(arg);
            var result = (argIndex >= 0) && (args.Count > argIndex + 1);
            return result;
        }

        public string GetOptionArgument(string arg, string defaultValue)
        {
            var result = defaultValue;
            if (HasOptionArgument(arg))
            {
                result = args[args.IndexOf(arg) + 1];
            }
            return result;
        }

    }
}
