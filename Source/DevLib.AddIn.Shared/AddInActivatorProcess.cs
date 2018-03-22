//-----------------------------------------------------------------------
// <copyright file="AddInActivatorProcess.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Security.Permissions;
    using System.Threading;

    /// <summary>
    /// Represents a process for AddInDomain and handles things such as attach/detach events and restarting the process.
    /// </summary>
    internal class AddInActivatorProcess : IDisposable
    {
        /// <summary>
        /// Const Field ConfigFileStringFormat.
        /// </summary>
        private const string ConfigFileStringFormat = @"{0}.exe.cfg";

        /// <summary>
        /// Const Field LogFileStringFormat.
        /// </summary>
        private const string LogFileStringFormat = @"{0}.exe.log";

        /// <summary>
        /// Readonly Field _addInDomainSetup.
        /// </summary>
        private readonly AddInDomainSetup _addInDomainSetup;

        /// <summary>
        /// Readonly Field _assemblyFile.
        /// </summary>
        private readonly string _assemblyFile;

        /// <summary>
        /// Readonly Field _process.
        /// </summary>
        private readonly Process _process;

        /// <summary>
        /// Readonly Field _friendlyName.
        /// </summary>
        private readonly string _friendlyName;

        /// <summary>
        /// Readonly Field _addInDomainSetupFile.
        /// </summary>
        private readonly string _addInDomainSetupFile;

        /// <summary>
        /// Readonly Field _addInDomainLogFile.
        /// </summary>
        private readonly string _addInDomainLogFile;

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Field _addInActivatorProcessInfo.
        /// </summary>
        private AddInActivatorProcessInfo _addInActivatorProcessInfo;

        /// <summary>
        /// Field _addInActivatorClient.
        /// </summary>
        private AddInActivatorClient _addInActivatorClient;

        /// <summary>
        /// Field _redirectOutput.
        /// </summary>
        private bool _redirectOutput;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddInActivatorProcess" /> class.
        /// </summary>
        /// <param name="friendlyName">Process name.</param>
        /// <param name="redirectOutput">Whether redirect console output.</param>
        /// <param name="addInDomainSetup">Instance of AddInDomainSetup.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public AddInActivatorProcess(string friendlyName, bool redirectOutput, AddInDomainSetup addInDomainSetup)
        {
            this._friendlyName = friendlyName;
            this._redirectOutput = redirectOutput;
            this._addInDomainSetup = addInDomainSetup;
            this._assemblyFile = AddInActivatorHostAssemblyCompiler.CreateRemoteHostAssembly(friendlyName, addInDomainSetup);
            this._addInDomainSetupFile = Path.Combine(addInDomainSetup.TempFilesDirectory, string.Format(ConfigFileStringFormat, friendlyName));
            this._addInDomainLogFile = Path.Combine(addInDomainSetup.TempFilesDirectory, string.Format(LogFileStringFormat, friendlyName));

            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.CreateNoWindow = true;
            processStartInfo.ErrorDialog = false;
            processStartInfo.FileName = this._assemblyFile;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.Verb = "runas";
            processStartInfo.WorkingDirectory = this._addInDomainSetup.WorkingDirectory;

            if (this._addInDomainSetup.EnvironmentVariables != null)
            {
                foreach (KeyValuePair<string, string> item in this._addInDomainSetup.EnvironmentVariables)
                {
                    processStartInfo.EnvironmentVariables[item.Key] = item.Value;
                }
            }

            this._process = new Process();
            this._process.StartInfo = processStartInfo;
            this._process.OutputDataReceived += this.OnProcessDataReceived;
            this._process.ErrorDataReceived += this.OnProcessDataReceived;
            this._process.Exited += this.OnProcessExited;
            this._process.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AddInActivatorProcess" /> class.
        /// </summary>
        ~AddInActivatorProcess()
        {
            this.IsFinalized = true;
            this.Kill();
            this.Dispose(false);
        }

        /// <summary>
        /// Delegate DeleteAssemblyFileDelegate.
        /// </summary>
        /// <param name="cancelEvent">Instance of ManualResetEvent.</param>
        private delegate void DeleteAssemblyFileDelegate(ManualResetEvent cancelEvent);

        /// <summary>
        /// Event Attached.
        /// </summary>
        public event EventHandler Attached;

        /// <summary>
        /// Event Detached.
        /// </summary>
        public event EventHandler Detached;

        /// <summary>
        /// Event DataReceived.
        /// </summary>
        public event DataReceivedEventHandler DataReceived;

        /// <summary>
        /// Gets a proxy to the remote AddInActivator to use to create remote object instances.
        /// </summary>
        public AddInActivator AddInActivatorClient
        {
            get
            {
                return this._addInActivatorClient != null ? this._addInActivatorClient.AddInActivator : null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the process is started and running.
        /// </summary>
        public bool IsRunning
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this instance of the <see cref="AddInActivatorProcess" /> class is finalized.
        /// </summary>
        public bool IsFinalized
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets instance of AddInActivatorProcessInfo.
        /// </summary>
        public AddInActivatorProcessInfo ProcessInfo
        {
            [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
            get
            {
                if (this._addInActivatorProcessInfo == null)
                {
                    this._addInActivatorProcessInfo = new AddInActivatorProcessInfo();
                }

                if (this._process != null)
                {
                    this._process.Refresh();

                    try
                    {
                        this._addInActivatorProcessInfo.BasePriority = this._process.BasePriority;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.BasePriority = -1;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.ExitCode = this._process.ExitCode;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.ExitCode = -1;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.ExitTime = this._process.ExitTime;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.ExitTime = DateTime.MinValue;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.HasExited = this._process.HasExited;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.HasExited = false;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.MachineName = this._process.MachineName;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.MachineName = string.Empty;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.MainWindowTitle = this._process.MainWindowTitle;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.MainWindowTitle = string.Empty;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.NonpagedSystemMemorySize64 = this._process.NonpagedSystemMemorySize64;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.NonpagedSystemMemorySize64 = -1;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.PagedMemorySize64 = this._process.PagedMemorySize64;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.PagedMemorySize64 = -1;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.PagedSystemMemorySize64 = this._process.PagedSystemMemorySize64;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.PagedSystemMemorySize64 = -1;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.PeakPagedMemorySize64 = this._process.PeakPagedMemorySize64;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.PeakPagedMemorySize64 = -1;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.PeakVirtualMemorySize64 = this._process.PeakVirtualMemorySize64;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.PeakVirtualMemorySize64 = -1;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.PeakWorkingSet64 = this._process.PeakWorkingSet64;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.PeakWorkingSet64 = -1;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.PID = this._process.Id;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.PID = -1;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.PrivateMemorySize64 = this._process.PrivateMemorySize64;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.PrivateMemorySize64 = -1;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.PrivilegedProcessorTime = this._process.PrivilegedProcessorTime;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.PrivilegedProcessorTime = TimeSpan.Zero;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.ProcessName = this._process.ProcessName;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.ProcessName = string.Empty;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.SessionId = this._process.SessionId;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.SessionId = -1;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.StartTime = this._process.StartTime;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.StartTime = DateTime.MinValue;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.TotalProcessorTime = this._process.TotalProcessorTime;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.TotalProcessorTime = TimeSpan.Zero;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.UserProcessorTime = this._process.UserProcessorTime;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.UserProcessorTime = TimeSpan.Zero;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.VirtualMemorySize64 = this._process.VirtualMemorySize64;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.VirtualMemorySize64 = -1;
                    }

                    try
                    {
                        this._addInActivatorProcessInfo.WorkingSet64 = this._process.WorkingSet64;
                    }
                    catch
                    {
                        this._addInActivatorProcessInfo.WorkingSet64 = -1;
                    }
                }

                return this._addInActivatorProcessInfo;
            }
        }

        /// <summary>
        /// Starts the remote process which will host an AddInActivator.
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public void Start()
        {
            this.CheckDisposed();

            this.DisposeClient();

            string guid = Guid.NewGuid().ToString();
            bool isCreated;

            using (EventWaitHandle serverStartedHandle = new EventWaitHandle(false, EventResetMode.ManualReset, string.Format(AddInActivatorHost.AddInDomainEventNameStringFormat, guid), out isCreated))
            {
                if (!isCreated)
                {
                    throw new Exception(AddInConstants.EventHandleAlreadyExistedException);
                }

                string addInDomainAssemblyPath = typeof(AddInActivatorProcess).Assembly.Location;

                AddInDomainSetup.WriteSetupFile(this._addInDomainSetup, this._addInDomainSetupFile);

                //// args[0] = AddInDomain assembly path
                //// args[1] = GUID
                //// args[2] = PID
                //// args[3] = AddInDomainSetup file
                //// args[4] = Redirect output or not

                this._process.StartInfo.Arguments = string.Format("\"{0}\" {1} {2} \"{3}\" {4}", addInDomainAssemblyPath, guid, Process.GetCurrentProcess().Id.ToString(), this._addInDomainSetupFile, this._redirectOutput.ToString());
                this.IsRunning = this._process.Start();

                if (!this.IsRunning)
                {
                    Debug.WriteLine(string.Format(AddInConstants.ProcessStartExceptionStringFormat, this._process.StartInfo.FileName));
                    throw new Exception(string.Format(AddInConstants.ProcessStartExceptionStringFormat, this._process.StartInfo.FileName));
                }

                if (!serverStartedHandle.WaitOne(this._addInDomainSetup.ProcessStartTimeout))
                {
                    Debug.WriteLine(AddInConstants.ProcessStartTimeoutException);
                    throw new Exception(AddInConstants.ProcessStartTimeoutException);
                }

                this._process.BeginOutputReadLine();
                this._process.BeginErrorReadLine();
                this._process.PriorityClass = this._addInDomainSetup.ProcessPriority;
                this._addInActivatorClient = new AddInActivatorClient(guid, this._addInDomainSetup);
                this.RaiseEvent(this.Attached);
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="AddInActivatorProcess" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="AddInActivatorProcess" /> class.
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="AddInActivatorProcess" /> class.
        /// protected virtual for non-sealed class; private for sealed class.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
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

                this.DisposeClient();

                this.Kill();

                if (this._process != null)
                {
                    this._process.Dispose();
                }
            }

            // free native resources
            ////if (nativeResource != IntPtr.Zero)
            ////{
            ////    Marshal.FreeHGlobal(nativeResource);
            ////    nativeResource = IntPtr.Zero;
            ////}

            this.IsRunning = false;
            this.RaiseEvent(this.Detached);
        }

        /// <summary>
        /// Kills the remote process.
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        private void Kill()
        {
            try
            {
                if (this._process != null && !this._process.HasExited)
                {
                    this._process.Exited -= this.OnProcessExited;
                    this._process.Kill();
                    this._process.WaitForExit(1000);
                }
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
            }

            if (this._addInDomainSetup.DeleteOnUnload)
            {
                try
                {
                    File.Delete(this._addInDomainLogFile);
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                }

                try
                {
                    File.Delete(this._addInDomainSetupFile);
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                }

                try
                {
                    File.Delete(this._assemblyFile);
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                }
            }
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
                temp(null, null);
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
                temp(null, e);
            }
        }

        /// <summary>
        /// Method OnProcessDataReceived.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Instance of DataReceivedEventArgs.</param>
        private void OnProcessDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                string output = string.Format(AddInConstants.ProcessOutputStringFormat, this._friendlyName, e.Data);

                Debug.WriteLine(output);

                this.RaiseDataReceivedEvent(e);

                if (this._redirectOutput)
                {
                    Console.WriteLine(output);
                }
            }
        }

        /// <summary>
        /// Method OnProcessExited.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Instance of EventArgs.</param>
        private void OnProcessExited(object sender, EventArgs e)
        {
            this.Dispose();
        }

        /// <summary>
        /// Method CheckDisposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.AddIn.AddInActivatorProcess");
            }
        }

        /// <summary>
        /// Method DisposeClient.
        /// </summary>
        private void DisposeClient()
        {
            if (this._addInActivatorClient != null)
            {
                this._addInActivatorClient.Dispose();
                this._addInActivatorClient = null;
            }
        }
    }
}
