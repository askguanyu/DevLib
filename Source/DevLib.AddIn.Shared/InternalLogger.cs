//-----------------------------------------------------------------------
// <copyright file="InternalLogger.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Internal logger.
    /// </summary>
    internal static class InternalLogger
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
        /// <param name="objs">Diagnostic messages or objects to log.</param>
        public static void Log(params object[] objs)
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
                            Console.WriteLine(message);
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
