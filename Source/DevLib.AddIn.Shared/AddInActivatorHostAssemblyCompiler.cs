//-----------------------------------------------------------------------
// <copyright file="AddInActivatorHostAssemblyCompiler.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Permissions;
    using Microsoft.CSharp;

    /// <summary>
    /// Generates an assembly to run in a separate process in order to host an AddInActivator.
    /// </summary>
    internal static class AddInActivatorHostAssemblyCompiler
    {
        /// <summary>
        /// Const Field OutputAssemblyFileStringFormat.
        /// </summary>
        private const string OutputAssemblyFileStringFormat = @"{0}.exe";

        /// <summary>
        /// Static Readonly Field ReferencedAssemblies.
        /// </summary>
        private static readonly string[] ReferencedAssemblies = new[] { "System.dll" };

        /// <summary>
        /// Static Method CreateRemoteHostAssembly.
        /// </summary>
        /// <param name="friendlyName">A name for the assembly.</param>
        /// <param name="addInDomainSetup">Instance of AddInDomainSetup.</param>
        /// <returns>The path of the assembly, or null if the assembly was generated in memory.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public static string CreateRemoteHostAssembly(string friendlyName, AddInDomainSetup addInDomainSetup)
        {
            if (!Directory.Exists(addInDomainSetup.TempFilesDirectory))
            {
                Directory.CreateDirectory(addInDomainSetup.TempFilesDirectory);
            }

            ////Dictionary<string, string> providerOptions = new Dictionary<string, string> { { "CompilerVersion", "v2.0" } };

            CompilerResults results = null;

            List<string> compilerArgs = new List<string> { AddInPlatformTarget.GetPlatformTargetCompilerArgument(addInDomainSetup.Platform) };
#if DEBUG
            compilerArgs.Add("/define:DEBUG");
#endif
            CompilerParameters compilerParameters = new CompilerParameters();
            compilerParameters.CompilerOptions = string.Join(" ", compilerArgs.ToArray());
            compilerParameters.GenerateExecutable = true;
            compilerParameters.GenerateInMemory = false;
            compilerParameters.OutputAssembly = Path.Combine(addInDomainSetup.TempFilesDirectory, string.Format(OutputAssemblyFileStringFormat, friendlyName));
            compilerParameters.ReferencedAssemblies.AddRange(ReferencedAssemblies);

            string assemblySource = DevLib.AddIn.Properties.Resources.Program.Replace("$[AddInActivatorHostTypeName]", typeof(AddInActivatorHost).AssemblyQualifiedName).Replace("$[AddInAssemblyName]", typeof(AddInActivatorHost).Assembly.FullName);

            ////using (CSharpCodeProvider provider = new CSharpCodeProvider(providerOptions))
            using (CSharpCodeProvider provider = new CSharpCodeProvider())
            {
                results = provider.CompileAssemblyFromSource(compilerParameters, assemblySource);
            }

            if (results.Errors.HasWarnings)
            {
                AddInAssemblyCompilerException addInAssemblyCompilerException = new AddInAssemblyCompilerException("Succeeded to compile assembly for AddInDomain with warnings.", results.Errors);

                Debug.WriteLine(string.Format(AddInConstants.WarningStringFormat, "DevLib.AddIn.AddInActivatorHostAssemblyCompiler.CreateRemoteHostAssembly", results.ToString(), addInAssemblyCompilerException.ToString(), results.Output.ToString(), string.Empty));
            }

            if (results.Errors.HasErrors)
            {
                AddInAssemblyCompilerException addInAssemblyCompilerException = new AddInAssemblyCompilerException("Failed to compile assembly for AddInDomain due to compiler errors.", results.Errors);

                Debug.WriteLine(string.Format(AddInConstants.ExceptionStringFormat, "DevLib.AddIn.AddInActivatorHostAssemblyCompiler.CreateRemoteHostAssembly", results.ToString(), addInAssemblyCompilerException.ToString(), results.Output.ToString(), string.Empty));

                throw addInAssemblyCompilerException;
            }

            return results.PathToAssembly;
        }
    }
}
