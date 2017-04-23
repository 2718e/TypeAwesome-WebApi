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
    public partial class TypescriptMaker
    {

        private static string QueryParamsToParameterList(ExtractedMethodInfo info)
        {
            if (info.QueryParamNames.Length == 0)
            {
                return "";
            }
            else
            {
                var result = Enumerable.Range(0, info.QueryParamNames.Length).Select(i => $"in{info.QueryParamNames[i]}: {info.QueryParamTypes[i]}").Aggregate((s, acc) => $"{s}, {acc}");
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
            bool isEnum = inParam.ParameterType.IsEnum;
            bool isSimple = SimpleTypes.Contains(inParam.ParameterType);
            bool hasConverter = inParam.ParameterType.CustomAttributes.Any(attr => attr.GetType() == typeof(TypeConverterAttribute));
            if (hasFromBody) { result = null; }
            else if (hasMapping) { result = PrimitiveTypeMappings[inParam.ParameterType]; }
            else if (isEnum) { result = $"E{inParam.ParameterType.Name}"; }
            else if (isSimple || hasConverter) { result = "string"; }
            else { result = null; };
            return result;
        }

        private static Dictionary<Type, string> AttributesToHttpMethods = new Dictionary<Type, string>
        {
            { typeof(HttpGetAttribute), "GET" },
            { typeof(HttpPostAttribute), "POST" },
            { typeof(HttpPutAttribute), "PUT" },
            { typeof(HttpDeleteAttribute), "DELETE" },
            { typeof(HttpHeadAttribute), "HEAD" },
            { typeof(HttpOptionsAttribute), "OPTIONS" },
            { typeof(HttpPatchAttribute), "PATCH" },
        };

        private static string[] ActionStarts = { "Get", "Post", "Put", "Delete", "Patch", "Options", "Head" };

        /// <summary>
        /// Gets the HTTP Method (Get, Post) that should be used to call the method.
        /// 
        /// According to https://www.exceptionnotfound.net/using-http-methods-correctly-in-asp-net-web-api/
        /// 
        /// 1) If there is an attribute applied (via [HttpGet], [HttpPost], [HttpPut], [AcceptVerbs], etc), the action will accept the specified HTTP method(s).
        /// 2) If the name of the controller action starts the words "Get", "Post", "Put", "Delete", "Patch", "Options", or "Head", use the corresponding HTTP method.
        /// 3) Otherwise, the action supports the POST method.
        /// 
        /// This method attempts to mostly follow the above logic, however
        /// - AcceptVerbs is not supported
        /// - if a controller action supports MULTIPLE Http Methods, only one of these will be returned
        /// </summary>
        /// <param name="methodInfo">THe controller method</param>
        /// <returns></returns>
        private string GetHttpMethodForMethod(MethodInfo methodInfo)
        {
            string result = null;
            foreach (Type attrType in AttributesToHttpMethods.Keys)
            {
                if (Attribute.IsDefined(methodInfo, attrType)) {
                    result = AttributesToHttpMethods[attrType];
                    break;
                }
            }
            if (result == null)
            {
                foreach(var actionStart in ActionStarts)
                {
                    if (methodInfo.Name.StartsWith(actionStart, StringComparison.OrdinalIgnoreCase))
                    {
                        result = actionStart.ToUpperInvariant();
                        break;
                    }
                }
            }
            result = result ?? "POST";
            return result;
        }

        private ExtractedMethodInfo ExtractMethodInfo(MethodInfo methodInfo)
        {
            var result = new ExtractedMethodInfo();
            result.Controller = methodInfo.DeclaringType.Name.Replace("Controller", "");
            result.Action = methodInfo.Name;
            result.BodyParamType = "void";
            result.BodyParamName = "parameter";
            result.ReturnType = ResolveCSharpType(methodInfo.ReturnType);
            result.HttpMethod = GetHttpMethodForMethod(methodInfo);
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
                result.BodyParamType = ResolveCSharpType(bodyParams[0].ParameterType);
                result.BodyParamName = bodyParams[0].Name;
            }
            result.QueryParamTypes = queryParamTypes.ToArray();
            result.QueryParamNames = queryParamNames.ToArray();
            return result;
        }
        /// <summary>
        /// Convenience class for extracting information from a System.Reflection.MethodInfo that TypeAwesomeWebApi actually uses.
        /// </summary>
        private class ExtractedMethodInfo
        {
            public string Controller { get; set; }
            public string Action { get; set; }
            public string BodyParamType { get; set; }
            public string BodyParamName { get; set; }
            public string ReturnType { get; set; }
            public string HttpMethod { get; set; }

            // each in a type/name pair have the same index.
            public string[] QueryParamTypes { get; set; }
            public string[] QueryParamNames { get; set; }

            public bool HasBodyParam()
            {
                return !BodyParamType.Equals("void", StringComparison.Ordinal);
            }


        }

    }
}
