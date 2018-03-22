//-----------------------------------------------------------------------
// <copyright file="AddInConstants.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    /// <summary>
    /// Static class AddInConstants
    /// </summary>
    internal static class AddInConstants
    {
        /// <summary>
        /// Const Field DefaultFriendlyName.
        /// </summary>
        internal const string DefaultFriendlyName = "AddInDomain";

        /// <summary>
        /// Const Field KeyIpcPortName.
        /// </summary>
        internal const string KeyIpcPortName = "portName";

        /// <summary>
        /// Const Field KeyIpcChannelName.
        /// </summary>
        internal const string KeyIpcChannelName = "name";

        /// <summary>
        /// Const Field IpcUrlStringFormat.
        /// </summary>
        internal const string IpcUrlStringFormat = "ipc://{0}/{1}";

        /// <summary>
        /// Const Field AddInUnknownPlatformException.
        /// </summary>
        internal const string AddInUnknownPlatformException = "Unknown platform target specified.";

        /// <summary>
        /// Const Field ExceptionStringFormat.
        /// </summary>
        internal const string ExceptionStringFormat = "[Exception:\r\n{0} failed with exception.\r\nSource:\r\n{1}\r\nMessage:\r\n{2}\r\nStackTrace:\r\n{3}\r\nRaw:\r\n{4}\r\n]";

        /// <summary>
        /// Const Field WarningStringFormat.
        /// </summary>
        internal const string WarningStringFormat = "[Warning:\r\n{0} failed with exception.\r\nSource:\r\n{1}\r\nMessage:\r\n{2}\r\nStackTrace:\r\n{3}\r\nRaw:\r\n{4}\r\n]";

        /// <summary>
        /// Const Field ProcessOutputStringFormat.
        /// </summary>
        internal const string ProcessOutputStringFormat = "[{0}] {1}";

        /// <summary>
        /// Const Field ProcessStartTimeoutException.
        /// </summary>
        internal const string ProcessStartTimeoutException = "Waiting for remote process to start timeout.";

        /// <summary>
        /// Const Field ProcessStartExceptionStringFormat.
        /// </summary>
        internal const string ProcessStartExceptionStringFormat = "Failed to start process from: {0}";

        /// <summary>
        /// Const Field AssemblyResolverException.
        /// </summary>
        internal const string AssemblyResolverException = "Could not load type for assembly resolver.";

        /// <summary>
        /// Const Field EventHandleNotExist.
        /// </summary>
        internal const string EventHandleNotExist = "Event handle did not exist for remote process.";

        /// <summary>
        /// Const Field EventHandleAlreadyExistedException.
        /// </summary>
        internal const string EventHandleAlreadyExistedException = "Event handle already existed for remote process.";

        /// <summary>
        /// Const Field DeleteFileExceptionStringFormat.
        /// </summary>
        internal const string DeleteFileExceptionStringFormat = "Failed to delete AddInDomain file '{0}'";
    }
}
