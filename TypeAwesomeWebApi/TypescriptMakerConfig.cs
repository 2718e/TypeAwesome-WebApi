using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeAwesomeWebApi
{
    public enum FilterNamespaceMode
    {
        Unspecified = 0,
        Include,
        Exclude
    }

    public class TypescriptMakerConfig
    {

        ///// <summary>
        ///// Whether to export as a namespace or a module. Defaults to module.
        ///// </summary>
        //public ModuleMode ModuleMode { get; set; }

        /// <summary>
        /// if NamespaceFilterMode is Include, will only export Model types from namespaces that start with a namespace name in FilteredNamespaces
        /// if NamespaceFilterMode is Exclude, will export Model types from all namespaces EXCEPT those that start with a namespace name in FilteredNamespaces
        /// if NamespaceFilterMode is Unspecified, will default to Exclude the namespace "System"
        /// </summary>
        public FilterNamespaceMode NamespaceFilterMode { get; set; }

        /// <summary>
        /// if NamespaceFilterMode is Include, will only export Model types from namespaces that start with a namespace name in FilteredNamespaces
        /// if NamespaceFilterMode is Exclude, will export Model types from all namespaces EXCEPT those that start with a namespace name in FilteredNamespaces
        /// if NamespaceFilterMode is Unspecified, will default to Exclude the namespace "System"
        /// </summary>
        public string[] FilteredNamespaces { get; set; }

        /// <summary>
        /// Assemblies containing controllers to export. Will throw an exception if null
        /// </summary>
        public Assembly[] ControllerAssemblies { get; set; }

        /// <summary>
        /// imports used by the generated code. (By default, jquery and lodash)
        /// </summary>
        public string[] ImportStatements { get; set; }

        /// <summary>
        /// Specify a non default header - this should contain alternative definitions for IMethodInfo, CallMethodNoBodyParam and CallMethodWithBodyParam.
        /// Possible reasons to do this could include wanting to use fetch instead of JQuery ajax, or adding custom headers to your requests.
        /// If null, defaults to templates/headers.tst
        /// </summary>
        public string HeaderOverride { get; set; }

        /// <summary>
        /// What should the name of the exported module be (defaults to ApiExport)
        /// </summary>
        public string ModuleName { get; set; }
    }
}
