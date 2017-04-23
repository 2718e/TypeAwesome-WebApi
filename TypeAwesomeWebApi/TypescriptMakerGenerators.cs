using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeAwesomeWebApi
{

    /// <summary>
    /// Section of the typescriptMaker class concerned with the actual generating of typescript
    /// </summary>
    public partial class TypescriptMaker
    {

        private void GenerateInterfaceForModel(Type model, StringBuilder exportBuilder)
        {
            var typeName = ResolveCSharpType(model);

            exportBuilder.AppendFormat("export interface {0} {1}", typeName, "{\r\n");
            var properties = model.GetProperties().ToList();
            foreach (var property in properties)
            {
                var propertyTypeName = ResolveCSharpType(property.PropertyType);
                var propertyName = property.Name;
                exportBuilder.Append($"  {propertyName}?: {propertyTypeName};\r\n");
            }
            exportBuilder.Append("}\r\n\r\n");
        }

        private void GenerateEnumForModel(Type model, StringBuilder exportBuilder)
        {
            var typeName = ResolveCSharpType(model);
            exportBuilder.Append($"export enum {typeName} {"{"}\r\n");
            List<string> fieldLines = new List<string>();
            foreach (var value in Enum.GetValues(model))
            {
                fieldLines.Add($"    {value} = {(int)value}");
            }
            exportBuilder.Append(fieldLines.Aggregate((s, acc) => $"{s},\r\n{acc}"));
            exportBuilder.Append("\r\n}\r\n\r\n");
        }

        private void GenerateTypesForModels(List<Type> typesToExport, StringBuilder exportBuilder)
        {
            foreach (var model in typesToExport)
            {
                var typeName = ResolveCSharpType(model);
                // any is returned from ResolveCSharpType if don't know how to process the type.
                // don't want to generate an interface in this case
                if (!typeName.Equals("any", StringComparison.Ordinal))
                {
                    if (model.IsEnum)
                    {
                        GenerateEnumForModel(model, exportBuilder);
                    }
                    else
                    {
                        GenerateInterfaceForModel(model, exportBuilder);
                    }
                }
            }
        }


        private void GenerateQueryParamInterfaces(List<MethodInfo> methods, StringBuilder exportBuilder)
        {
            foreach (var method in methods)
            {
                var info = ExtractMethodInfo(method);
                var queryParams = method.GetParameters().Where(p => GetQueryStringParamType(p) != null).ToList();
                if (info.QueryParamNames.Length > 0)
                {
                    var typeName = QueryParamsTypeName(info);
                    exportBuilder.AppendFormat("export interface {0} {1}", typeName, "{\r\n");
                    for (int i = 0; i < info.QueryParamNames.Length; i++)
                    {
                        exportBuilder.Append($"  {info.QueryParamNames[i]}: {info.QueryParamTypes[i]};\r\n");
                    }
                    exportBuilder.Append("}\r\n\r\n");
                }
            }
        }

        private static string QueryParamsTypeName(ExtractedMethodInfo info)
        {
            if (info.QueryParamNames.Length > 0)
            {
                return $"{info.Controller}{info.Action}QueryParams";
            }
            else { return "{}"; }
        }

        private static string MethodInfoName(ExtractedMethodInfo info)
        {
            return $"{info.Controller}{info.Action}MethodInfo";
        }

        private void GenerateIMethodInfos(List<MethodInfo> methods, StringBuilder exportBuilder)
        {
            var extracts = methods.Select(m => ExtractMethodInfo(m)).ToList();
            foreach (var info in extracts)
            {
                exportBuilder.Append($"export const {MethodInfoName(info)}: IMethodInfo<{info.BodyParamType}, {QueryParamsTypeName(info)}, {info.ReturnType}> = {"{"}\r\n");
                exportBuilder.Append($"    url: \"/api/{info.Controller}/{info.Action}\",\r\n");
                exportBuilder.Append($"    httpMethod: \"{info.HttpMethod}\"\r\n{"}"};\r\n\r\n");
            }
        }

        private void GenerateBodylessCallers(List<MethodInfo> methods, StringBuilder exportBuilder)
        {
            var infos = methods.Select(m => ExtractMethodInfo(m)).Where(info => !info.HasBodyParam()).ToList();
            foreach (var info in infos)
            {
                var queryParamsList = QueryParamsToParameterList(info);
                var queryParamsObject = QueryParamsObjectFromList(info);
                exportBuilder.Append($"export function {info.Controller}{info.Action}({queryParamsList}): PromiseLike<{info.ReturnType}>{" {\r\n"}");
                exportBuilder.Append($"    let queryParams = {queryParamsObject};\r\n");
                exportBuilder.Append($"    return CallMethodNoBodyParam({MethodInfoName(info)}, queryParams);\r\n{"}"}\r\n\r\n");
            }
        }

        private void GenerateBodyCallers(List<MethodInfo> methods, StringBuilder exportBuilder)
        {
            var infos = methods.Select(m => ExtractMethodInfo(m)).Where(info => info.HasBodyParam()).ToList();
            foreach (var info in infos)
            {
                var queryParamsList = QueryParamsToParameterList(info);
                var queryParamsObject = QueryParamsObjectFromList(info);
                var commaOrNot = queryParamsList.Equals(String.Empty, StringComparison.Ordinal) ? "" : ",";
                exportBuilder.Append($"export function {info.Controller}{info.Action}({queryParamsList}{commaOrNot} {info.BodyParamName}: {info.BodyParamType}): PromiseLike<{info.ReturnType}>{" {\r\n"}");
                exportBuilder.Append($"    let queryParams = {queryParamsObject};\r\n");
                exportBuilder.Append($"    return CallMethodWithBodyParam({MethodInfoName(info)}, {info.BodyParamName}, queryParams);\r\n{"}"}\r\n\r\n");
            }
        }


    }
}
