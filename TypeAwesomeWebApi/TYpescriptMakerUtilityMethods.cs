using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeAwesomeWebApi
{

    /// <summary>
    /// Short Utility function used by TypescriptMaker
    /// </summary>
    public partial class TypescriptMaker
    {

        private bool ShouldExportModelType(Type t)
        {
            var result = !(t.IsPrimitive || t == typeof(string));
            switch (config.NamespaceFilterMode)
            {
                case FilterNamespaceMode.Exclude:
                    result = result && !(this.config.FilteredNamespaces.Any(excluded => t.Namespace.ToUpperInvariant().StartsWith(excluded.ToUpperInvariant())));
                    break;
                case FilterNamespaceMode.Include:
                    result = result && this.config.FilteredNamespaces.Any(excluded => t.Namespace.ToUpperInvariant().StartsWith(excluded.ToUpperInvariant()));
                    break;
                default:
                    result = result && !t.Namespace.ToUpperInvariant().StartsWith("SYSTEM");
                    break;
            }
            return result;
        }

        /// <summary>
        /// Adds the type, and all types of public properties of that type, to the set (if they have not already been added).
        /// 
        /// Adds recursively, e.g. if a public property is of a type that itself has public properties, these will also be added, etc.
        /// </summary>
        /// <param name="t">the type to add</param>
        /// <param name="set">the set to add the type to</param>
        private void AddTypeToSetRecursive(Type t, ISet<Type> set)
        {
            if (t.HasElementType) { throw new InvalidOperationException("something went wrong, AddTypeToSetRecursive should never be called with collection types"); }
            if (!set.Contains(t) && ShouldExportModelType(t))
            {
                set.Add(t);
                t.GetProperties().Select(p => RemoveWrapperTypes(p.PropertyType)).ToList().ForEach(pType => AddTypeToSetRecursive(pType, set));
            }
        }

        /// <summary>
        /// Gets all types used as parameters or return values in all the methods. Returns a set with no duplicates
        /// </summary>
        /// <param name="methods">the methods to extract types from</param>
        /// <returns>a set of types used by all the methods</returns>
        private ISet<Type> GetTypesReferenced(IList<MethodInfo> methods)
        {
            var resultSet = new HashSet<Type>();
            methods.SelectMany(GetTypesForMethod).ToList().ForEach(t => AddTypeToSetRecursive(t, resultSet));
            return resultSet;
        }

        /// <summary>
        /// Converts a type that may or may not be a collection to it's element type (if it is a collection). Operates repeatedly on multiple layers of wrappers - e.g. an int[][][][] will be converted to int
        /// </summary>
        /// <param name="inType">The type to extract from</param>
        /// <returns>if inType is not a collection, returns inType, otherwise gets the element type repeatedly until a type that is not a collection is found, and returns that</returns>
        private static Type RemoveCollectionWrappers(Type inType)
        {
            var t = inType;
            while (t.HasElementType)
            {
                t = t.GetElementType();
            }
            return t;
        }

        /// <summary>
        /// Returns the underlying type if the input type is nullable, e.g. int? becomes just int. This only applies to types that are not usually nullable - e.g. a string would still be string.
        /// </summary>
        /// <param name="inType">the possibly nullable type to unwrap</param>
        /// <returns>if inType is not explicitly nullable, returns inType, otherwise returns the corresponding non nullable type</returns>
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

        /// <summary>
        /// Calls both RemoveCollectionWrappers and RemoveNullableWrappers repeatedly until a type that is neither nullable nor a collection is found
        /// </summary>
        /// <param name="inType">The type to unwrap</param>
        /// <returns>the unwrapped type, which will be the first underlying non nullable/ element type if the input is (or wraps) a nullable or collection type</returns>
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

        /// <summary>
        /// Gets all types taken as parameter or returned from a method.
        /// </summary>
        /// <param name="method">The method to get types for</param>
        /// <returns>A list of types passed as parameter to, or returned from, the method</returns>
        private static List<Type> GetTypesForMethod(MethodInfo method)
        {
            var usedTypes = method.GetParameters().Select(p => p.ParameterType).ToList();
            var strippedTypes = usedTypes.Select(RemoveWrapperTypes);
            var result = strippedTypes.ToList();
            result.Add(RemoveWrapperTypes(method.ReturnType));
            return result;
        }


       

    }
}
