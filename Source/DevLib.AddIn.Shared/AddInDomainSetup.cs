//-----------------------------------------------------------------------
// <copyright file="AddInDomainSetup.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Permissions;
    using System.Security.Policy;

    /// <summary>
    /// Class AddInDomainSetup.
    /// </summary>
    [Serializable]
    public sealed class AddInDomainSetup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddInDomainSetup" /> class.
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public AddInDomainSetup()
        {
            this.AppDomainSetup = this.CloneDeep(AppDomain.CurrentDomain.SetupInformation);
            this.DeleteOnUnload = true;
            this.DllDirectory = Directory.GetCurrentDirectory();
            this.EnvironmentVariables = new Dictionary<string, string>();
            this.Evidence = AppDomain.CurrentDomain.Evidence;
            this.ExternalAssemblies = new Dictionary<AssemblyName, string>();
            this.Platform = PlatformTargetEnum.AnyCPU;
            this.ProcessPriority = ProcessPriorityClass.Normal;
            this.ProcessStartTimeout = new TimeSpan(0, 0, 15);
            this.RestartOnProcessExit = true;
            this.ShadowCopyDirectories = this.AppDomainSetup.ApplicationBase;
            this.TempFilesDirectory = Path.GetTempPath();
            this.TypeFilterLevel = TypeFilterLevel.Full;
            this.UseShadowCopy = true;
            this.WorkingDirectory = Environment.CurrentDirectory;
        }

        /// <summary>
        /// Gets or sets where the temporary remote process executable file and other files will be created.
        /// </summary>
        public string TempFilesDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the working directory for the remote process.
        /// </summary>
        public string WorkingDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a directory to redirect DLL probing to the working directory.
        /// </summary>
        public string DllDirectory
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets how long to wait for the remote process to start.
        /// </summary>
        public TimeSpan ProcessStartTimeout
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not to delete the generated executable after AddInDomain has unloaded.
        /// </summary>
        public bool DeleteOnUnload
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether AddInDomain should be relaunched when the process exit prematurely.
        /// </summary>
        public bool RestartOnProcessExit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets setup information for the AppDomain that the object will be created in, in the remote process.
        /// By default, this will be the current domain's setup information from which the proxy is being created.
        /// </summary>
        public AppDomainSetup AppDomainSetup
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the list of directories under the application base directory that are probed for private assemblies.
        /// </summary>
        /// <remarks>
        /// The list of directory names, where each name is separated by a semicolon.
        /// </remarks>
        public string PrivateBinPath
        {
            get
            {
                return this.AppDomainSetup != null ? this.AppDomainSetup.PrivateBinPath : string.Empty;
            }

            set
            {
                if (this.AppDomainSetup != null)
                {
                    this.AppDomainSetup.PrivateBinPath = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the names of the directories containing assemblies to be shadow copied.
        /// </summary>
        /// <remarks>
        /// The list of directory names, where each name is separated by a semicolon.
        /// </remarks>
        public string ShadowCopyDirectories
        {
            get
            {
                return this.AppDomainSetup != null ? this.AppDomainSetup.ShadowCopyDirectories : string.Empty;
            }

            set
            {
                if (this.AppDomainSetup != null)
                {
                    this.AppDomainSetup.ShadowCopyDirectories = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether shadow copying is turned on or off.
        /// </summary>
        public bool UseShadowCopy
        {
            get
            {
                if (this.AppDomainSetup != null)
                {
                    return this.AppDomainSetup.ShadowCopyFiles.Equals("true", StringComparison.OrdinalIgnoreCase) ? true : false;
                }

                return false;
            }

            set
            {
                if (this.AppDomainSetup != null)
                {
                    this.AppDomainSetup.ShadowCopyFiles = value ? "true" : "false";
                }
            }
        }

        /// <summary>
        /// Gets or sets which platform to compile the target remote process assembly for.
        /// </summary>
        public PlatformTargetEnum Platform
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets remote security policy.
        /// </summary>
        public Evidence Evidence
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the level of automatic deserialization for .NET Framework remoting.
        /// </summary>
        public TypeFilterLevel TypeFilterLevel
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets environment variables of the remote process.
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a dictionary of assembly names to assembly file locations that will need to be resolved inside AddInDomain.
        /// </summary>
        public Dictionary<AssemblyName, string> ExternalAssemblies
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the priority to run the remote process at.
        /// </summary>
        public ProcessPriorityClass ProcessPriority
        {
            get;
            set;
        }

        /// <summary>
        /// Static Method WriteSetupFile.
        /// </summary>
        /// <param name="addInDomainSetup">Instance of AddInDomainSetup.</param>
        /// <param name="filename">Setup file name.</param>
        internal static void WriteSetupFile(AddInDomainSetup addInDomainSetup, string filename)
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using (FileStream fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                formatter.Serialize(fileStream, addInDomainSetup);
            }
        }

        /// <summary>
        /// Static Method ReadSetupFile.
        /// </summary>
        /// <param name="filename">Setup file name.</param>
        /// <returns>Instance of AddInDomainSetup.</returns>
        internal static AddInDomainSetup ReadSetupFile(string filename)
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using (FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                return (AddInDomainSetup)formatter.Deserialize(fileStream);
            }
        }

        /// <summary>
        /// Perform a deep Copy of the object.
        /// </summary>
        /// <typeparam name="T">The type of input object.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        private T CloneDeep<T>(T source)
        {
            if (source == null)
            {
                return default(T);
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, source);
                memoryStream.Position = 0;
                return (T)binaryFormatter.Deserialize(memoryStream);
            }
        }
    }
}
