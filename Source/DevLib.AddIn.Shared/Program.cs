//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Class Program.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:ElementsMustBeOrderedByAccess", Justification = "Reviewed.")]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1204:StaticElementsMustAppearBeforeInstanceElements", Justification = "Reviewed.")]
    internal class Program
    {
        /// <summary>
        /// Method Main, entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        private static void Main(string[] args)
        {
            //// args[0] = AddInDomain assembly path
            //// args[1] = GUID
            //// args[2] = PID
            //// args[3] = AddInDomainSetup file
            //// args[4] = Redirect output or not

            if (args.Length < 4)
            {
                ProgramInternalLogger.Log(true, new ArgumentNullException("args"));

                Console.WriteLine(@"
args[0] = AddInDomain assembly path
args[1] = GUID
args[2] = PID
args[3] = AddInDomainSetup file
args[4] = Redirect output or not");

                return;
            }

            bool redirectOutput = false;

            bool.TryParse(args[4], out redirectOutput);

            try
            {
                ProgramInternalLogger.Log(redirectOutput, "AddInDomain is started");
                ProgramInternalLogger.Log(redirectOutput, Environment.CommandLine);

                Dictionary<AssemblyName, string> resolveDict = new Dictionary<AssemblyName, string>();
                resolveDict.Add(new AssemblyName("$[AddInAssemblyName]"), args[0]);

                AssemblyResolver resolver = new AssemblyResolver(resolveDict);

                resolver.Mount();

                Type hostType = Type.GetType("$[AddInActivatorHostTypeName]");

                if (hostType != null)
                {
                    ProgramInternalLogger.Log(redirectOutput, "Succeeded: Type.GetType($[AddInActivatorHostTypeName])");
                }
                else
                {
                    ProgramInternalLogger.Log(redirectOutput, string.Format("Could not load AddInActivatorHost type $[AddInActivatorHostTypeName] by using resolver with $[AddInAssemblyName] mapped to {0}", args[0]));
                    throw new TypeLoadException(string.Format("Could not load AddInActivatorHost type $[AddInActivatorHostTypeName] by using resolver with $[AddInAssemblyName] mapped to {0}", args[0]));
                }

                MethodInfo methodInfo = hostType.GetMethod("Run", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(string[]) }, null);

                if (methodInfo != null)
                {
                    ProgramInternalLogger.Log(redirectOutput, "Succeeded: GetMethod on AddInActivatorHost");
                }
                else
                {
                    ProgramInternalLogger.Log(redirectOutput, "'Run' method on AddInActivatorHost was not found.");
                    throw new Exception("'Run' method on AddInActivatorHost was not found.");
                }

                ProgramInternalLogger.Log(redirectOutput, "Begin Invoke AddInActivatorHost method.");

                methodInfo.Invoke(null, new object[] { args });
            }
            catch (Exception e)
            {
                ProgramInternalLogger.Log(redirectOutput, string.Format("Summary: Failed to launch AddInActivatorHost:\r\n{0}", e.ToString()));
            }
        }
    }

    /// <summary>
    /// Class AssemblyResolver.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed.")]
    public class AssemblyResolver : MarshalByRefObject
    {
        /// <summary>
        /// Readonly Field _assemblyDictionary.
        /// </summary>
        private readonly Dictionary<string, Dictionary<AssemblyName, string>> _assemblyDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyResolver" /> class.
        /// </summary>
        /// <param name="dict">Instance of Dictionary.</param>
        public AssemblyResolver(Dictionary<AssemblyName, string> dict)
        {
            this._assemblyDictionary = new Dictionary<string, Dictionary<AssemblyName, string>>();

            if (dict != null)
            {
                foreach (KeyValuePair<AssemblyName, string> item in dict)
                {
                    Dictionary<AssemblyName, string> subDict;

                    if (!this._assemblyDictionary.TryGetValue(item.Key.Name, out subDict))
                    {
                        this._assemblyDictionary[item.Key.Name] = subDict = new Dictionary<AssemblyName, string>();
                    }

                    subDict[item.Key] = item.Value;
                }
            }
        }

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
        /// Subscribe events.
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public void Mount()
        {
            AppDomain.CurrentDomain.AssemblyResolve += this.Resolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += this.ReflectionOnlyResolve;
        }

        /// <summary>
        /// Unsubscribe events.
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public void Unmount()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= this.Resolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= this.ReflectionOnlyResolve;
        }

        /// <summary>
        /// Static Method PublicKeysTokenEqual.
        /// </summary>
        /// <param name="left">Left token.</param>
        /// <param name="right">Right token.</param>
        /// <returns>true if equals; otherwise, false.</returns>
        private static bool PublicKeysTokenEqual(byte[] left, byte[] right)
        {
            if (left == null || right == null)
            {
                return left == right;
            }

            if (left.Length != right.Length)
            {
                return false;
            }

            for (int i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Method Resolve.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="args">Instance of ResolveEventArgs.</param>
        /// <returns>Instance of Assembly.</returns>
        private Assembly Resolve(object sender, ResolveEventArgs args)
        {
            string assemblyFile = this.FindAssemblyName(args.Name);

            if (assemblyFile != null)
            {
                return Assembly.LoadFrom(assemblyFile);
            }

            return null;
        }

        /// <summary>
        /// Method ReflectionOnlyResolve.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="args">Instance of ResolveEventArgs.</param>
        /// <returns>Instance of Assembly.</returns>
        private Assembly ReflectionOnlyResolve(object sender, ResolveEventArgs args)
        {
            string assemblyFile = this.FindAssemblyName(args.Name);

            if (assemblyFile != null)
            {
                return Assembly.ReflectionOnlyLoadFrom(assemblyFile);
            }

            return null;
        }

        /// <summary>
        /// Method FindAssemblyName.
        /// </summary>
        /// <param name="name">The display name of the assembly, as returned by the <see cref="P:System.Reflection.AssemblyName.FullName" /> property.</param>
        /// <returns>AssemblyName string.</returns>
        private string FindAssemblyName(string name)
        {
            AssemblyName assemblyName = new AssemblyName(name);
            Dictionary<AssemblyName, string> subDict;

            if (!this._assemblyDictionary.TryGetValue(assemblyName.Name, out subDict))
            {
                return null;
            }

            foreach (KeyValuePair<AssemblyName, string> item in subDict)
            {
                if (assemblyName.Version != null && !assemblyName.Version.Equals(item.Key.Version))
                {
                    continue;
                }

                if (assemblyName.CultureInfo != null && !assemblyName.CultureInfo.Equals(item.Key.CultureInfo))
                {
                    continue;
                }

                if (!PublicKeysTokenEqual(assemblyName.GetPublicKeyToken(), item.Key.GetPublicKeyToken()))
                {
                    continue;
                }

                return item.Value;
            }

            return null;
        }
    }

    /// <summary>
    /// Program internal logger.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed.")]
    internal static class ProgramInternalLogger
    {
        /// <summary>
        /// Field ExecutingAssembly.
        /// </summary>
        private static readonly string ExecutingAssembly = Path.GetFullPath(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

        /// <summary>
        /// Field GlobalDebugFlagFile.
        /// </summary>
        private static readonly string GlobalDebugFlagFile = Path.Combine(Path.GetDirectoryName(ExecutingAssembly), "DevLib#Debug");

        /// <summary>
        /// Field DebugFlagFile.
        /// </summary>
        private static readonly string DebugFlagFile = ExecutingAssembly + "#Debug";

        /// <summary>
        /// Field LogFile.
        /// </summary>
        private static readonly string LogFile = ExecutingAssembly + ".log";

        /// <summary>
        /// Field LogFileBackup.
        /// </summary>
        private static readonly string LogFileBackup = Path.ChangeExtension(LogFile, ".log.bak");

        /// <summary>
        /// Field SyncRoot.
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// Method Log.
        /// </summary>
        /// <param name="redirectOutput">Redirect output to current console or not.</param>
        /// <param name="objs">Diagnostic messages or objects to log.</param>
        public static void Log(bool redirectOutput, params object[] objs)
        {
#if DEBUG
            if (objs != null)
            {
                lock (SyncRoot)
                {
                    if (objs != null)
                    {
                        try
                        {
                            string message = RenderLog(objs);
                            Debug.WriteLine(message);

                            if (redirectOutput)
                            {
                                Console.WriteLine(message);
                            }

                            AppendToFile(message);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e.ToString());
                            Console.WriteLine(e.ToString());
                        }
                    }
                }
            }
#else
            if (File.Exists(GlobalDebugFlagFile) || File.Exists(DebugFlagFile))
            {
                if (objs != null)
                {
                    lock (SyncRoot)
                    {
                        if (objs != null)
                        {
                            try
                            {
                                string message = RenderLog(objs);

                                if (redirectOutput)
                                {
                                    Console.WriteLine(message);
                                }

                                AppendToFile(message);
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Builds a readable representation of the stack trace.
        /// </summary>
        /// <param name="skipFrames">The number of frames up the stack to skip.</param>
        /// <returns>A readable representation of the stack trace.</returns>
        public static string GetStackFrameInfo(int skipFrames)
        {
            StackFrame stackFrame = new StackFrame(skipFrames < 1 ? 1 : skipFrames + 1, true);

            MethodBase method = stackFrame.GetMethod();

            if (method != null)
            {
                StringBuilder result = new StringBuilder();

                result.Append(method.Name);

                if (method is MethodInfo && ((MethodInfo)method).IsGenericMethod)
                {
                    Type[] genericArguments = ((MethodInfo)method).GetGenericArguments();

                    result.Append("<");

                    int i = 0;

                    bool flag = true;

                    while (i < genericArguments.Length)
                    {
                        if (!flag)
                        {
                            result.Append(",");
                        }
                        else
                        {
                            flag = false;
                        }

                        result.Append(genericArguments[i].Name);

                        i++;
                    }

                    result.Append(">");
                }

                result.Append(" in ");
                result.Append(Path.GetFileName(stackFrame.GetFileName()) ?? "<unknown>");
                result.Append(":");
                result.Append(stackFrame.GetFileLineNumber());

                return result.ToString();
            }
            else
            {
                return "<null>";
            }
        }

        /// <summary>
        /// Render parameters into a string.
        /// </summary>
        /// <param name="objs">Diagnostic messages or objects to log.</param>
        /// <returns>The rendered layout string.</returns>
        private static string RenderLog(object[] objs)
        {
            StringBuilder result = new StringBuilder();

            result.Append(DateTimeOffset.Now.ToString("o", CultureInfo.InvariantCulture));
            result.Append("|INTL|");
            result.Append(Environment.UserName);
            result.Append("|");
            result.Append(Thread.CurrentThread.ManagedThreadId.ToString("000"));
            result.Append("|");

            if (objs != null && objs.Length > 0)
            {
                foreach (object item in objs)
                {
                    result.Append(" [");
                    result.Append(item == null ? string.Empty : item.ToString());
                    result.Append("]");
                }
            }

            result.Append(" |");
            result.Append(GetStackFrameInfo(2));
            result.Append(Environment.NewLine);

            return result.ToString();
        }

        /// <summary>
        /// Append log message to the file.
        /// </summary>
        /// <param name="message">Log message to append.</param>
        private static void AppendToFile(string message)
        {
            try
            {
            }
            finally
            {
                FileStream fileStream = null;

                try
                {
                    fileStream = new FileStream(LogFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);

                    if (fileStream.Length > 10485760)
                    {
                        try
                        {
                            File.Copy(LogFile, LogFileBackup, true);
                        }
                        catch
                        {
                        }

                        fileStream.SetLength(0);
                    }

                    byte[] bytes = Encoding.UTF8.GetBytes(message);

                    fileStream.Seek(0, SeekOrigin.End);
                    fileStream.Write(bytes, 0, bytes.Length);
                    fileStream.Flush();
                }
                catch
                {
                }
                finally
                {
                    if (fileStream != null)
                    {
                        fileStream.Dispose();
                        fileStream = null;
                    }
                }
            }
        }
    }
}
