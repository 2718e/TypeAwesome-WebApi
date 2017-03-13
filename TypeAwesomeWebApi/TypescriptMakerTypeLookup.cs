using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace TypeAwesomeWebApi
{

    /// <summary>
    /// Part of the TypescriptMaker class concerned with determining the corresponding typescript type name from a C# type.
    /// </summary>
    public partial class TypescriptMaker
    {

        /// <summary>
        /// how the primitive types will be converted
        /// </summary>
        private static Dictionary<Type, string> PrimitiveTypeMappings = new Dictionary<Type, string>
        {
            // TODO add all primitive types (e.g. Int16) not just commmon aliases
            { typeof(bool), "boolean" },
            { typeof(char), "string" },
            { typeof(string), "string" },
            { typeof(float), "number" },
            { typeof(double), "number" },
            { typeof(decimal), "number" },
            { typeof(byte), "number" },
            { typeof(int), "number" },
            { typeof(long), "number" },
        };
        private static bool IsNullable(Type t)
        {
            return (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }


        /// <summary>
        /// Converts A C# type, as found in the property of a model class to a typescript type
        /// </summary>
        /// <param name="cSharpType">the type in c shart</param>
        /// <returns></returns>
        private string ResolveCSharpType(Type cSharpType)
        {
            var result = "";
            
            if (PrimitiveTypeMappings.ContainsKey(cSharpType))
            {
                result = PrimitiveTypeMappings[cSharpType];
            }
            else if (cSharpType.HasElementType)
            {
                var typeofArray = ResolveCSharpType(cSharpType.GetElementType());
                // TODO use indexer if a string dictionary.
                result = $"{typeofArray}[]";
            }
            else if (cSharpType.IsEnum)
            {
                result = $"E{cSharpType.Name}";
            }
            else if (SimpleTypes.Contains(cSharpType))
            {
                result = "string";
            }
            else if (!ShouldExportModelType(cSharpType))
            {
                result = "any";
            }
            else if (!(cSharpType.GenericTypeArguments.Length > 0))
            {
                result = $"I{cSharpType.Name}";
            }
            else if (IsNullable(cSharpType))
            {
                result = $"{ResolveCSharpType(Nullable.GetUnderlyingType(cSharpType))} | null";
            }
            else
            {
                result = "any";
            }
            return result;
        }

        private static ISet<Type> SimpleTypes = new HashSet<Type>
        {
            typeof(TimeSpan), typeof(DateTime), typeof(Guid)
        };




    }
}
