//-----------------------------------------------------------------------
// <copyright file="AddInDomain.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Threading;

    /// <summary>
    /// Represents an isolated environment in a separate process in which objects can be created and invoked.
    /// </summary>
    public sealed class AddInDomain : IDisposable
    {
        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Field _addInActivatorProcess.
        /// </summary>
        private AddInActivatorProcess _addInActivatorProcess;

        /// <summary>
        /// Field _unloaded.
        /// </summary>
        private int _unloaded;

        /// <summary>
        /// Field _overloadCreateInstanceAndUnwrap.
        /// </summary>
        private int _overloadCreateInstanceAndUnwrap = 0;

        /// <summary>
        /// Field _addInAssemblyName.
        /// </summary>
        private string _addInAssemblyName;

        /// <summary>
        /// Field _addInArgs.
        /// </summary>
        private object[] _addInArgs = null;

        /// <summary>
        /// Field _addInActivationAttributes.
        /// </summary>
        private object[] _addInActivationAttributes = null;

        /// <summary>
        /// Field _addInIgnoreCase.
        /// </summary>
        private bool _addInIgnoreCase;

        /// <summary>
        /// Field _addInBindingAttr.
        /// </summary>
        private BindingFlags _addInBindingAttr = BindingFlags.Default;

        /// <summary>
        /// Field _addInBinder.
        /// </summary>
        private Binder _addInBinder = null;

        /// <summary>
        /// Field _addInCulture.
        /// </summary>
        private CultureInfo _addInCulture = null;

        /// <summary>
        /// Field _addInSecurityAttributes.
        /// </summary>
        private Evidence _addInSecurityAttributes = null;

        /// <summary>
        /// Field _canRestart.
        /// </summary>
        private bool _canRestart = true;

        /// <summary>
        /// Field _addInTypeName.
        /// </summary>
        private string _addInTypeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddInDomain" /> class.
        /// </summary>
        /// <param name="friendlyName">The friendly name of the AddInDomain.</param>
        /// <param name="showRedirectConsoleOutput">Whether the output of AddInActivatorProcess is shown in current console.</param>
        /// <param name="addInDomainSetup">Additional settings for creating AddInDomain.</param>
        public AddInDomain(string friendlyName = null, bool showRedirectConsoleOutput = true, AddInDomainSetup addInDomainSetup = null)
        {
            this.FriendlyName = string.IsNullOrEmpty(friendlyName) ? AddInConstants.DefaultFriendlyName : friendlyName;
            this.AddInDomainSetupInfo = addInDomainSetup ?? new AddInDomainSetup();
            this.RedirectOutput = showRedirectConsoleOutput;
            this.CreateAddInActivatorProcess();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AddInDomain" /> class.
        /// </summary>
        ~AddInDomain()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Event Loaded.
        /// </summary>
        public event EventHandler Loaded;

        /// <summary>
        /// Event Unloaded.
        /// </summary>
        public event EventHandler Unloaded;

        /// <summary>
        /// Event Reloaded.
        /// </summary>
        public event EventHandler Reloaded;

        /// <summary>
        /// Occurs when AddInDomain writes to its redirected <see cref="P:System.Diagnostics.Process.StandardOutput" /> stream.
        /// </summary>
        public event DataReceivedEventHandler DataReceived;

        /// <summary>
        /// Gets or sets a value indicating whether the output of AddInActivatorProcess is shown in current console.
        /// </summary>
        public bool RedirectOutput
        {
            get;
            set;
        }

        /// <summary>
        /// Gets AddInActivatorProcessInfo.
        /// </summary>
        public AddInActivatorProcessInfo ProcessInfo
        {
            get
            {
                if (this._addInActivatorProcess != null)
                {
                    return this._addInActivatorProcess.ProcessInfo;
                }
                else
                {
                    return new AddInActivatorProcessInfo();
                }
            }
        }

        /// <summary>
        /// Gets AddIn object.
        /// </summary>
        public object AddInObject
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the friendly name of the AddInDomain.
        /// </summary>
        public string FriendlyName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets AddInDomainSetup information.
        /// </summary>
        public AddInDomainSetup AddInDomainSetupInfo
        {
            get;
            set;
        }

        /// <summary>
        /// Method Reload.
        /// </summary>
        public void Reload()
        {
            this.Unload();
            this.RestartAddInActivatorProcess();
        }

        /// <summary>
        /// Method Unload.
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public void Unload()
        {
            this._canRestart = false;

            if (Interlocked.CompareExchange(ref this._unloaded, 1, 0) == 1)
            {
                return;
            }

            if (this._addInActivatorProcess != null)
            {
                this._addInActivatorProcess.Attached -= this.OnProcessAttached;
                this._addInActivatorProcess.Detached -= this.OnProcessDetached;
                this._addInActivatorProcess.DataReceived -= this.OnDataReceived;
                this._addInActivatorProcess.Dispose();
                this._addInActivatorProcess = null;
            }
        }

        /// <summary>
        /// Creates a new instance of the specified type.
        /// </summary>
        /// <param name="assemblyName">The display name of the assembly. See <see cref="P:System.Reflection.Assembly.FullName" />.</param>
        /// <param name="typeName">The fully qualified name of the requested type, including the namespace but not the assembly, as returned by the <see cref="P:System.Type.FullName" /> property.</param>
        /// <returns>An instance of the object specified by <paramref name="typeName" />.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public object CreateInstanceAndUnwrap(string assemblyName, string typeName)
        {
            this.CheckDisposed();

            this.StartAddInActivatorProcess();

            this._overloadCreateInstanceAndUnwrap = 1;
            this._addInAssemblyName = assemblyName;
            this._addInTypeName = typeName;

            this.AddInObject = this._addInActivatorProcess.AddInActivatorClient.CreateInstanceAndUnwrap(this._addInAssemblyName, this._addInTypeName);

            return this.AddInObject;
        }

        /// <summary>
        /// Creates a new instance of the specified type.
        /// </summary>
        /// <param name="assemblyName">The display name of the assembly. See <see cref="P:System.Reflection.Assembly.FullName" />.</param>
        /// <param name="typeName">The fully qualified name of the requested type, including the namespace but not the assembly, as returned by the <see cref="P:System.Type.FullName" /> property.</param>
        /// <param name="activationAttributes">An array of one or more attributes that can participate in activation. Typically, an array that contains a single <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> object. The <see cref="T:System.Runtime.Remoting.Activation.UrlAttribute" /> specifies the URL that is required to activate a remote object.</param>
        /// <returns>An instance of the object specified by <paramref name="typeName" />.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public object CreateInstanceAndUnwrap(string assemblyName, string typeName, object[] activationAttributes)
        {
            this.CheckDisposed();

            this.StartAddInActivatorProcess();

            this._overloadCreateInstanceAndUnwrap = 2;
            this._addInAssemblyName = assemblyName;
            this._addInTypeName = typeName;
            this._addInActivationAttributes = activationAttributes;

            this.AddInObject = this._addInActivatorProcess.AddInActivatorClient.CreateInstanceAndUnwrap(this._addInAssemblyName, this._addInTypeName, this._addInActivationAttributes);

            return this.AddInObject;
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
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public object CreateInstanceAndUnwrap(string assemblyName, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes, Evidence securityAttributes)
        {
            this.CheckDisposed();

            this.StartAddInActivatorProcess();

            this._overloadCreateInstanceAndUnwrap = 3;
            this._addInAssemblyName = assemblyName;
            this._addInTypeName = typeName;
            this._addInIgnoreCase = ignoreCase;
            this._addInBindingAttr = bindingAttr;
            this._addInBinder = binder;
            this._addInArgs = args;
            this._addInCulture = culture;
            this._addInActivationAttributes = activationAttributes;
            this._addInSecurityAttributes = securityAttributes;
            this.AddInObject = this._addInActivatorProcess.AddInActivatorClient.CreateInstanceAndUnwrap(
                                                                                                        this._addInAssemblyName,
                                                                                                        this._addInTypeName,
                                                                                                        this._addInIgnoreCase,
                                                                                                        this._addInBindingAttr,
                                                                                                        this._addInBinder,
                                                                                                        this._addInArgs,
                                                                                                        this._addInCulture,
                                                                                                        this._addInActivationAttributes,
                                                                                                        this._addInSecurityAttributes);

            return this.AddInObject;
        }

        /// <summary>
        /// Creates an object of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of instance.</typeparam>
        /// <returns>An instance of the object.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public T CreateInstance<T>()
        {
            return (T)this.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName);
        }

        /// <summary>
        /// Creates an object of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of instance.</typeparam>
        /// <param name="args">The arguments to pass to the constructor. This array of arguments must match in number, order, and type the parameters of the constructor to invoke. If the default constructor is preferred, <paramref name="args" /> must be an empty array or null. </param>
        /// <returns>An instance of the object.</returns>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public T CreateInstance<T>(params object[] args)
        {
            return (T)this.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName, true, BindingFlags.Default, null, args, null, null, null);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="AddInDomain" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="AddInDomain" /> class.
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="AddInDomain" /> class.
        /// protected virtual for non-sealed class; private for sealed class.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (this._disposed)
            {
                return;
            }

            this._disposed = true;

            if (disposing)
            {
                // dispose managed resources
                ////if (managedResource != null)
                ////{
                ////    managedResource.Dispose();
                ////    managedResource = null;
                ////}

                this.Unload();
            }

            // free native resources
            ////if (nativeResource != IntPtr.Zero)
            ////{
            ////    Marshal.FreeHGlobal(nativeResource);
            ////    nativeResource = IntPtr.Zero;
            ////}
        }

        /// <summary>
        /// Method CreateAddInActivatorProcess.
        /// </summary>
        private void CreateAddInActivatorProcess()
        {
            this._canRestart = true;

            this._addInActivatorProcess = new AddInActivatorProcess(this.FriendlyName, this.RedirectOutput, this.AddInDomainSetupInfo);
            this._addInActivatorProcess.Attached += this.OnProcessAttached;
            this._addInActivatorProcess.Detached += this.OnProcessDetached;
            this._addInActivatorProcess.DataReceived += this.OnDataReceived;
        }

        /// <summary>
        /// Method StartAddInActivatorProcess.
        /// </summary>
        private void StartAddInActivatorProcess()
        {
            if (this._addInActivatorProcess != null && !this._addInActivatorProcess.IsRunning)
            {
                this._addInActivatorProcess.Start();
            }
        }

        /// <summary>
        /// Method RestartAddInActivatorProcess.
        /// </summary>
        private void RestartAddInActivatorProcess()
        {
            this.CreateAddInActivatorProcess();

            this.StartAddInActivatorProcess();

            if (this.AddInObject != null)
            {
                try
                {
                    switch (this._overloadCreateInstanceAndUnwrap)
                    {
                        case 1:
                            this.AddInObject = this._addInActivatorProcess.AddInActivatorClient.CreateInstanceAndUnwrap(this._addInAssemblyName, this._addInTypeName);
                            break;

                        case 2:
                            this.AddInObject = this._addInActivatorProcess.AddInActivatorClient.CreateInstanceAndUnwrap(this._addInAssemblyName, this._addInTypeName, this._addInActivationAttributes);
                            break;

                        case 3:
                            this.AddInObject = this._addInActivatorProcess.AddInActivatorClient.CreateInstanceAndUnwrap(
                                this._addInAssemblyName,
                                this._addInTypeName,
                                this._addInIgnoreCase,
                                this._addInBindingAttr,
                                this._addInBinder,
                                this._addInArgs,
                                this._addInCulture,
                                this._addInActivationAttributes,
                                this._addInSecurityAttributes);
                            break;

                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                    throw;
                }
            }

            this.RaiseEvent(this.Reloaded);
        }

        /// <summary>
        /// Method RaiseEvent.
        /// </summary>
        /// <param name="eventHandler">Instance of EventHandler.</param>
        private void RaiseEvent(EventHandler eventHandler)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety.
            EventHandler temp = Interlocked.CompareExchange(ref eventHandler, null, null);

            if (temp != null)
            {
                temp(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Method RaiseDataReceivedEvent.
        /// </summary>
        /// <param name="e">Instance of DataReceivedEventArgs.</param>
        private void RaiseDataReceivedEvent(DataReceivedEventArgs e)
        {
            // Copy a reference to the delegate field now into a temporary field for thread safety.
            DataReceivedEventHandler temp = Interlocked.CompareExchange(ref this.DataReceived, null, null);

            if (temp != null)
            {
                temp(this, e);
            }
        }

        /// <summary>
        /// Method OnProcessAttached.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Instance of EventArgs.</param>
        private void OnProcessAttached(object sender, EventArgs e)
        {
            this.RaiseEvent(this.Loaded);
        }

        /// <summary>
        /// Method OnProcessDetached.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Instance of EventArgs.</param>
        private void OnProcessDetached(object sender, EventArgs e)
        {
            this.RaiseEvent(this.Unloaded);

            if (!this._addInActivatorProcess.IsFinalized && this.AddInDomainSetupInfo.RestartOnProcessExit && this._canRestart)
            {
                this.RestartAddInActivatorProcess();
            }
        }

        /// <summary>
        /// Method OnDataReceived.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Instance of DataReceivedEventArgs.</param>
        private void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.RaiseDataReceivedEvent(e);
        }

        /// <summary>
        /// Method CheckDisposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.AddIn.AddInDomain");
            }
        }
    }
}
