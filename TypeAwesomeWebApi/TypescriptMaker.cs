using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace TypeAwesomeWebApi
{


    [Serializable]
    public class InvalidTypeExportException : Exception
    {
        public InvalidTypeExportException() { }
        public InvalidTypeExportException(string message) : base(message) { }
        public InvalidTypeExportException(string message, Exception inner) : base(message, inner) { }
        protected InvalidTypeExportException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    public static class TypescriptMaker
    {

        // when figuring out what types to export, only want the element type for collection types -
        // will just map collection types to arrays. Dictionaries not handled at the moment. 
        private static Type RemoveCollectionWrappers(Type inType)
        {
            var t = inType;
            while (t.HasElementType)
            {
                t = t.GetElementType();
            }
            return t;
        }

        private static Type RemoveNullableWrapper(Type inType)
        {
            if (IsNullable(inType)) // System.Nullable cannot take another nullable as argument, so don't need a while here
            {
                return Nullable.GetUnderlyingType(inType);
            }
            else
            {
                return inType;
            }
        }

        private static Type RemoveWrapperTypes(Type inType)
        {
            var t = inType;
            while (t.HasElementType || IsNullable(t))
            {
                t = RemoveCollectionWrappers(t);
                t = RemoveNullableWrapper(t);
            }
            return t;
        }

        private static List<Type> GetTypesForMethod(MethodInfo method)
        {
            var usedTypes = method.GetParameters().Select(p => p.ParameterType).ToList();
            var strippedTypes = usedTypes.Select(RemoveWrapperTypes);
            var result = strippedTypes.ToList();
            result.Add(RemoveWrapperTypes(method.ReturnType));
            return result;
        }

        /// <summary>
        /// Adds the type, and all types of public properties of that type, to the set (if they have not already been added).
        /// 
        /// Adds recursively, e.g. if a public property is of a type that itself has public properties, these will also be added, etc.
        /// </summary>
        /// <param name="t">the type to add</param>
        /// <param name="set">the set to add the type to</param>
        private static void AddTypeToSetRecursive(Type t, ISet<Type> set)
        {
            if (t.HasElementType) { throw new InvalidOperationException("something went wrong, AddTypeToSetRecursive should never be called with collection types"); }
            if (!(t.IsPrimitive || t == typeof(string) || set.Contains(t)))
            {
                set.Add(t);
                t.GetProperties().Select(p => RemoveWrapperTypes(p.PropertyType)).ToList().ForEach(pType => AddTypeToSetRecursive(pType, set));
            }
        }

        public static ISet<Type> GetTypesReferenced(IList<MethodInfo> methods)
        {
            var resultSet = new HashSet<Type>();
            methods.SelectMany(GetTypesForMethod).ToList().ForEach(t => AddTypeToSetRecursive(t, resultSet));
            return resultSet;
        }

        /// <summary>
        /// how the primitive types will be converted
        /// </summary>
        private static Dictionary<Type, string> PrimitiveTypeMappings = new Dictionary<Type, string>
        {
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
        /// <param name="cSharpType">the type of</param>
        /// <param name="inModelNameTemplate"></param>
        /// <returns></returns>
        private static string ResolveCSharpType(Type cSharpType)
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



        private static void GenerateModelInterfaces(List<Type> typesToExport, StringBuilder exportBuilder)
        {
            foreach (var model in typesToExport)
            {
                var typeName = ResolveCSharpType(model);
                // any is returned from ResolveCSharpType if don't know how to process the type.
                // don't want to generate an interface in this case
                if (!typeName.Equals("any", StringComparison.Ordinal))
                {
                    exportBuilder.AppendFormat("export interface {0} {1}", typeName, "{\r\n");
                    var properties = model.GetProperties().ToList();
                    foreach (var property in properties)
                    {
                        var propertyTypeName = ResolveCSharpType(property.PropertyType);
                        var propertyName = property.Name;
                        exportBuilder.Append($"  {propertyName} : {propertyTypeName};\r\n");
                    }
                    exportBuilder.Append("}\r\n\r\n");
                }
            }
        }

        private static ISet<Type> SimpleTypes = new HashSet<Type>
        {
            typeof(TimeSpan), typeof(DateTime), typeof(Guid)
        };

        /// <summary>
        /// gets what the type for a query string param should be.
        /// 
        /// If this parameter would be taken from the body instead, returns null.
        /// 
        /// For primitive types, resolves using the primitive type mapping table,
        /// 
        /// otherwise, returns "string" (ie the user is responsible for encoding)
        /// </summary>
        /// <param name="inParam"></param>
        private static string GetQueryStringParamType(ParameterInfo inParam)
        {
            string result;
            bool hasFromBody = inParam.CustomAttributes.Any(attr => attr.GetType() == typeof(FromBodyAttribute));
            bool hasMapping = PrimitiveTypeMappings.Keys.Contains(inParam.ParameterType);
            bool isSimple = SimpleTypes.Contains(inParam.ParameterType);
            bool hasConverter = inParam.ParameterType.CustomAttributes.Any(attr => attr.GetType() == typeof(TypeConverterAttribute));
            if (hasFromBody) { result = null; }
            else if (hasMapping) { result = PrimitiveTypeMappings[inParam.ParameterType]; }
            else if (isSimple || hasConverter) { result = "string"; }
            else { result = null; };
            return result;
        }

        private static string QueryParamsTypeName(ExtractedMethodInfo info)
        {
            if (info.QueryParamNames.Length > 0)
            {
                return $"{info.Controller}{info.Action}QueryParams";
            }
            else { return "{}"; }
        }

        private static void GenerateQueryParamInterfaces(List<MethodInfo> methods, StringBuilder exportBuilder)
        {
            foreach (var method in methods)
            {
                var info = new ExtractedMethodInfo(method);
                var queryParams = method.GetParameters().Where(p => GetQueryStringParamType(p) != null).ToList();
                if (info.QueryParamNames.Length > 0)
                {
                    var typeName = QueryParamsTypeName(info);
                    exportBuilder.AppendFormat("export interface {0} {1}", typeName, "{\r\n");
                    for (int i = 0; i < info.QueryParamNames.Length; i++)
                    {
                        exportBuilder.Append($"  {info.QueryParamNames[i]} : {info.QueryParamTypes[i]};\r\n");
                    }
                    exportBuilder.Append("}\r\n\r\n");
                }
            }
        }

        private class ExtractedMethodInfo
        {
            public string Controller { get; private set; }
            public string Action { get; private set; }
            public string BodyParamType { get; private set; }
            public string BodyParamName { get; private set; }
            public string ReturnType { get; private set; }

            // each in a type/name pair have the same index.
            public string[] QueryParamTypes { get; private set; }
            public string[] QueryParamNames { get; private set; }

            public bool HasBodyParam()
            {
                return !BodyParamType.Equals("void", StringComparison.Ordinal);
            }

            public ExtractedMethodInfo(MethodInfo methodInfo)
            {
                Controller = methodInfo.DeclaringType.Name.Replace("Controller", "");
                Action = methodInfo.Name;
                BodyParamType = "void";
                BodyParamName = "parameter";
                ReturnType = ResolveCSharpType(methodInfo.ReturnType);
                var queryParamNames = new List<string> { };
                var queryParamTypes = new List<string> { };
                var queryParams = methodInfo.GetParameters().Where(p => GetQueryStringParamType(p) != null).ToList();
                foreach (var queryParam in queryParams)
                {
                    queryParamNames.Add(queryParam.Name);
                    queryParamTypes.Add(GetQueryStringParamType(queryParam));
                }
                var bodyParams = methodInfo.GetParameters().Where(p => GetQueryStringParamType(p) == null).ToList();
                if (bodyParams.Count > 1)
                {
                    throw new InvalidTypeExportException($"method {methodInfo.Name} in {methodInfo.DeclaringType.Name} seems to expect multiple parameters from the request body - by default Web Api does not allow this.");
                }
                if (bodyParams.Count == 1)
                {
                    BodyParamType = ResolveCSharpType(bodyParams[0].ParameterType);
                    BodyParamName = bodyParams[0].Name;
                }
                QueryParamTypes = queryParamTypes.ToArray();
                QueryParamNames = queryParamNames.ToArray();
            }
        }

        private static string MethodInfoName(ExtractedMethodInfo info)
        {
            return $"{info.Controller}{info.Action}MethodInfo";
        }

        private static void GenerateIMethodInfos(List<MethodInfo> methods, StringBuilder exportBuilder)
        {
            var extracts = methods.Select(m => new ExtractedMethodInfo(m)).ToList();
            foreach (var info in extracts)
            {
                exportBuilder.Append($"export var {MethodInfoName(info)} : IMethodInfo<{info.BodyParamType},{QueryParamsTypeName(info)},{info.ReturnType}> = {"{\r\n"}");
                exportBuilder.Append($"    url : \"api/{info.Controller}/{info.Action}\"{"\r\n}\r\n\r\n"}");
            }
        }

        private static string QueryParamsToParameterList(ExtractedMethodInfo info)
        {
            if (info.QueryParamNames.Length == 0)
            {
                return "";
            }
            else
            {
                var result = Enumerable.Range(0, info.QueryParamNames.Length).Select(i => $"in{info.QueryParamNames[i]} : {info.QueryParamTypes[i]}").Aggregate((s, acc) => $"{s}, {acc}");
                return result;
            }
        }
        private static string QueryParamsObjectFromList(ExtractedMethodInfo info)
        {
            if (info.QueryParamNames.Length == 0)
            {
                return "{}";
            }
            else
            {
                var fieldList = Enumerable.Range(0, info.QueryParamNames.Length).Select(i => $"{info.QueryParamNames[i]} : in{info.QueryParamNames[i]}").Aggregate((s, acc) => $"{s}, {acc}");
                var result = $"{"{"}{fieldList}{"}"}";
                return result;
            }
        }

        private static void GenerateBodylessCallers(List<MethodInfo> methods, StringBuilder exportBuilder)
        {
            var infos = methods.Select(m => new ExtractedMethodInfo(m)).Where(info => !info.HasBodyParam()).ToList();
            foreach(var info in infos)
            {
                var queryParamsList = QueryParamsToParameterList(info);
                var queryParamsObject = QueryParamsObjectFromList(info);
                exportBuilder.Append($"export function {info.Controller}{info.Action}({queryParamsList}) : PromiseLike<{info.ReturnType}>{"{\r\n"}");
                exportBuilder.Append($"var queryParams = {queryParamsObject};");
                exportBuilder.Append($"return CallMethodNoBodyParam({MethodInfoName(info)}, queryParams) \r\n{"}"}\r\n\r\n");
            }
        }

        private static void GenerateBodyCallers(List<MethodInfo> methods, StringBuilder exportBuilder)
        {
            var infos = methods.Select(m => new ExtractedMethodInfo(m)).Where(info => info.HasBodyParam()).ToList();
            foreach (var info in infos)
            {
                var queryParamsList = QueryParamsToParameterList(info);
                var queryParamsObject = QueryParamsObjectFromList(info);
                var commaOrNot = queryParamsList.Equals(String.Empty, StringComparison.Ordinal) ? "" : ",";
                exportBuilder.Append($"export function {info.Controller}{info.Action}({queryParamsList}{commaOrNot} {info.BodyParamName} : {info.BodyParamType}) : PromiseLike<{info.ReturnType}>{"{\r\n"}");
                exportBuilder.Append($"var queryParams = {queryParamsObject};");
                exportBuilder.Append($"return CallMethodWithBodyParam({MethodInfoName(info)}, {info.BodyParamName}, queryParams) \r\n{"}"}\r\n\r\n");
            }
        }


        public static string MakeScriptsFrom(IEnumerable<Assembly> assemblies, string moduleName)
        {
            var types = assemblies.SelectMany(asm => asm.ExportedTypes).ToList();
            var controllers = types.Where(inType => inType.IsSubclassOf(typeof(ApiController))).ToList();
            var methodsToExport = controllers.SelectMany(controller => controller.GetMethods().Where(m=>m.DeclaringType == controller)).ToList();
            var typesToExport = GetTypesReferenced(methodsToExport).ToList();
            StringBuilder resultBuilder = new StringBuilder();
            resultBuilder.AppendLine(@"import * as _ from 'lodash';");
            resultBuilder.AppendLine(@"import * as $ from 'jquery';");
            resultBuilder.AppendLine();
            resultBuilder.Append($"module {moduleName} {"{\r\n\r\n"}");
            // insert the rest of the stuff
            resultBuilder.Append(File.ReadAllText(Path.Combine(".", "templates", "headers.tst")));
            GenerateModelInterfaces(typesToExport, resultBuilder);
            GenerateQueryParamInterfaces(methodsToExport, resultBuilder);
            GenerateIMethodInfos(methodsToExport, resultBuilder);
            GenerateBodylessCallers(methodsToExport, resultBuilder);
            GenerateBodyCallers(methodsToExport, resultBuilder);
            resultBuilder.Append("}\r\n");
            resultBuilder.Append($"export = {moduleName};");
            return resultBuilder.ToString();
        }

    }
}
