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

    public partial class TypescriptMaker
    {

        private TypescriptMakerConfig config { get; set; }

        public TypescriptMaker(TypescriptMakerConfig inConfig)
        {
            this.config = inConfig;
        }

        private string GetDefaultHeaders()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(typeof(TypescriptMaker), "templates.headers.tst");
            var s = new StreamReader(stream).ReadToEnd();
            return s;
        }

        public string MakeTypescript()
        {
            var moduleName = this.config.ModuleName ?? "ApiExport";
            var types = this.config.ControllerAssemblies.SelectMany(asm => asm.ExportedTypes).ToList();
            var controllers = types.Where(inType => inType.IsSubclassOf(typeof(ApiController))).ToList();
            var methodsToExport = controllers.SelectMany(controller => controller.GetMethods().Where(m => m.DeclaringType == controller)).ToList();
            var typesToExport = GetTypesReferenced(methodsToExport).ToList();
            StringBuilder resultBuilder = new StringBuilder();
            var tsImports = config.ImportStatements ?? new string[] { "import * as _ from \"lodash\";", "import * as $ from \"jquery\";" };
            foreach (string tsImport in tsImports)
            {
                resultBuilder.AppendLine(tsImport);
            }
            resultBuilder.AppendLine();
            resultBuilder.Append($"module {moduleName} {"{\r\n\r\n"}");
            // insert the rest of the stuff
            resultBuilder.Append(this.config.HeaderOverride ?? GetDefaultHeaders());
            GenerateTypesForModels(typesToExport, resultBuilder);
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
