//-----------------------------------------------------------------------
// <copyright file="AddInActivator.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Security.Policy;

    /// <summary>
    /// Represents an application domain, which is an isolated environment where applications execute.
    /// </summary>
    [Serializable]
    internal class AddInActivator : MarshalByRefObject
    {
        /// <summary>
        /// Gives the <see cref="T:System.AppDomain" /> an infinite lifetime by preventing a lease from being created.
        /// </summary>
        /// <exception cref="T:System.AppDomainUnloadedException">The operation is attempted on an unloaded application domain.</exception>
        /// <returns>Always null.</returns>
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }

        /// <summary>
        /// Creates a new instance of the specified type. Parameters specify the assembly where the type is defined, and the name of the type.
        /// </summary>
        /// <param name="assemblyName">The display name of the assembly. See <see cref="P:System.Reflection.Assembly.FullName" />.</param>
        /// <param name="typeName">The fully qualified name of the requested type, including the namespace but not the assembly, as returned by the <see cref="P:System.Type.FullName" /> property.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="assemblyName" /> or <paramref name="typeName" /> is null. </exception>
        /// <exception cref="T:System.MissingMethodException">No matching public constructor was found. </exception>
        /// <exception cref="T:System.TypeLoadException">
        ///   <paramref name="typeName" /> was not found in <paramref name="assemblyName" />. </exception>
        /// <exception cref="T:System.IO.FileNotFoundException">
        ///   <paramref name="assemblyName" /> was not found. </exception>
        /// <exception cref="T:System.MethodAccessException">The caller does not have permission to call this constructor. </exception>
        /// <exception cref="T:System.AppDomainUnloadedException">The operation is attempted on an unloaded application domain. </exception>
        /// <exception cref="T:System.BadImageFormatException">
        ///   <paramref name="assemblyName" /> is not a valid assembly. -or-Version 2.0 or later of the common language runtime is currently loaded and <paramref name="assemblyName" /> was compiled with a later version.</exception>
        /// <exception cref="T:System.IO.FileLoadException">An assembly or module was loaded twice with two different evidences. </exception>
        /// <returns>An instance of the object specified by <paramref name="typeName" />.</returns>
        public object CreateInstanceAndUnwrap(string assemblyName, string typeName)
        {
            return AppDomain.CurrentDomain.CreateInstanceAndUnwrap(assemblyName, typeName);
        }

        /// <summary>
        /// Creates a new instance of the specified type. Parameters specify the assembly where the type is defined, the name of the type, and an array of activation attributes.
        /// </summary>
        /// <param name="assemblyName">The display name of the assembly. See <see cref="P:System.Reflection.Assembly.FullName" />.</param>
        /// <param name="typeName">The fully qualified name of the requested type, including the namespace but not the assembly, as returned by the <see cref="P:System.Type.FullName" /> property.</param>
        /// <param name="activationAttributes">An array of one or more attributes that can participate in activation. Typically, an array that contains a single <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> object. The <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> specifies the URL that is required to activate a remote object.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="assemblyName" /> or <paramref name="typeName" /> is null. </exception>
        /// <exception cref="T:System.MissingMethodException">No matching public constructor was found. </exception>
        /// <exception cref="T:System.TypeLoadException">
        ///   <paramref name="typeName" /> was not found in <paramref name="assemblyName" />. </exception>
        /// <exception cref="T:System.IO.FileNotFoundException">
        ///   <paramref name="assemblyName" /> was not found. </exception>
        /// <exception cref="T:System.MethodAccessException">The caller does not have permission to call this constructor. </exception>
        /// <exception cref="T:System.NotSupportedException">The caller cannot provide activation attributes for an object that does not inherit from <see cref="T:System.MarshalByRefObject" />. </exception>
        /// <exception cref="T:System.AppDomainUnloadedException">The operation is attempted on an unloaded application domain. </exception>
        /// <exception cref="T:System.BadImageFormatException">
        ///   <paramref name="assemblyName" /> is not a valid assembly. -or-Version 2.0 or later of the common language runtime is currently loaded and <paramref name="assemblyName" /> was compiled with a later version.</exception>
        /// <exception cref="T:System.IO.FileLoadException">An assembly or module was loaded twice with two different evidences. </exception>
        /// <returns>An instance of the object specified by <paramref name="typeName" />.</returns>
        public object CreateInstanceAndUnwrap(string assemblyName, string typeName, object[] activationAttributes)
        {
            return AppDomain.CurrentDomain.CreateInstanceAndUnwrap(assemblyName, typeName, activationAttributes);
        }

        /// <summary>Creates a new instance of the specified type defined in the specified assembly, specifying whether the case of the type name is ignored; the binding attributes and the binder that are used to select the type to be created; the arguments of the constructor; the culture; and the activation attributes.</summary>
        /// <param name="assemblyName">The display name of the assembly. See <see cref="P:System.Reflection.Assembly.FullName" />.</param>
        /// <param name="typeName">The fully qualified name of the requested type, including the namespace but not the assembly, as returned by the <see cref="P:System.Type.FullName" /> property. </param>
        /// <param name="ignoreCase">A Boolean value specifying whether to perform a case-sensitive search or not. </param>
        /// <param name="bindingAttr">A combination of zero or more bit flags that affect the search for the <paramref name="typeName" /> constructor. If <paramref name="bindingAttr" /> is zero, a case-sensitive search for public constructors is conducted. </param>
        /// <param name="binder">An object that enables the binding, coercion of argument types, invocation of members, and retrieval of <see cref="T:System.Reflection.MemberInfo" /> objects using reflection. If <paramref name="binder" /> is null, the default binder is used. </param>
        /// <param name="args">The arguments to pass to the constructor. This array of arguments must match in number, order, and type the parameters of the constructor to invoke. If the default constructor is preferred, <paramref name="args" /> must be an empty array or null. </param>
        /// <param name="culture">A culture-specific object used to govern the coercion of types. If <paramref name="culture" /> is null, the CultureInfo for the current thread is used. </param>
        /// <param name="activationAttributes">An array of one or more attributes that can participate in activation. Typically, an array that contains a single <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> object. The <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> specifies the URL that is required to activate a remote object. </param>
        /// <param name="securityAttributes">Information used to authorize creation of <paramref name="typeName" />.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="assemblyName" /> or <paramref name="typeName" /> is null. </exception>
        /// <exception cref="T:System.MissingMethodException">No matching constructor was found. </exception>
        /// <exception cref="T:System.TypeLoadException">
        ///   <paramref name="typeName" /> was not found in <paramref name="assemblyName" />. </exception>
        /// <exception cref="T:System.IO.FileNotFoundException">
        ///   <paramref name="assemblyName" /> was not found. </exception>
        /// <exception cref="T:System.MethodAccessException">The caller does not have permission to call this constructor. </exception>
        /// <exception cref="T:System.NotSupportedException">The caller cannot provide activation attributes for an object that does not inherit from <see cref="T:System.MarshalByRefObject" />. </exception>
        /// <exception cref="T:System.AppDomainUnloadedException">The operation is attempted on an unloaded application domain. </exception>
        /// <exception cref="T:System.BadImageFormatException">
        ///   <paramref name="assemblyName" /> is not a valid assembly. -or-<paramref name="assemblyName" /> was compiled with a later version of the common language runtime than the version that is currently loaded.</exception>
        /// <exception cref="T:System.IO.FileLoadException">An assembly or module was loaded twice with two different evidences. </exception>
        /// <returns>An instance of the object specified by <paramref name="typeName" />.</returns>
        public object CreateInstanceAndUnwrap(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityAttributes)
        {
            return AppDomain.CurrentDomain.CreateInstanceAndUnwrap(assemblyName, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, securityAttributes);
        }
    }
}
