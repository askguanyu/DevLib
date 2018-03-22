//-----------------------------------------------------------------------
// <copyright file="AddInActivatorClient.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System;
    using System.Collections;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Channels.Ipc;
    using System.Security.Permissions;

    /// <summary>
    /// Provides access to AddInActivator in a remote process.
    /// </summary>
    internal class AddInActivatorClient : IDisposable
    {
        /// <summary>
        /// Readonly Field _addInActivator.
        /// </summary>
        private readonly AddInActivator _addInActivator;

        /// <summary>
        /// Readonly Field _ipcChannel.
        /// </summary>
        private readonly IpcChannel _ipcChannel;

        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddInActivatorClient" /> class.
        /// </summary>
        /// <param name="guid">Guid string.</param>
        /// <param name="addInDomainSetup">Instance of AddInDomainSetup.</param>
        [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public AddInActivatorClient(string guid, AddInDomainSetup addInDomainSetup)
        {
            BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
            serverProvider.TypeFilterLevel = addInDomainSetup.TypeFilterLevel;

            BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();

            Hashtable properties = new Hashtable();
            properties[AddInConstants.KeyIpcPortName] = string.Format(AddInActivatorHost.AddInClientChannelNameStringFormat, guid);
            properties[AddInConstants.KeyIpcChannelName] = string.Format(AddInActivatorHost.AddInClientChannelNameStringFormat, guid);

            this._ipcChannel = new IpcChannel(properties, clientProvider, serverProvider);
            ChannelServices.RegisterChannel(this._ipcChannel, false);

            this._addInActivator = (AddInActivator)Activator.GetObject(typeof(AddInActivator), string.Format(AddInConstants.IpcUrlStringFormat, string.Format(AddInActivatorHost.AddInServerChannelNameStringFormat, guid), AddInActivatorHost.AddInActivatorName));
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AddInActivatorClient" /> class.
        /// </summary>
        ~AddInActivatorClient()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets instance of AddInActivator.
        /// </summary>
        public AddInActivator AddInActivator
        {
            get { return this._addInActivator; }
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="AddInActivatorClient" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="AddInActivatorClient" /> class.
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="AddInActivatorClient" /> class.
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

                if (this._ipcChannel != null)
                {
                    try
                    {
                        this._ipcChannel.StopListening(null);
                        ChannelServices.UnregisterChannel(this._ipcChannel);
                    }
                    catch
                    {
                    }
                }
            }

            // free native resources
            ////if (nativeResource != IntPtr.Zero)
            ////{
            ////    Marshal.FreeHGlobal(nativeResource);
            ////    nativeResource = IntPtr.Zero;
            ////}
        }

        /// <summary>
        /// Method CheckDisposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.AddIn.AddInActivatorClient");
            }
        }
    }
}
