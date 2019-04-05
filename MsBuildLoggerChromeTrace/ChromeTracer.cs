using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace MsBuildLoggerChromeTrace
{
    /// <summary>
    /// Low level methods for ChromeTracer functionalities
    /// </summary>
    public class ChromeTracer
    {
        /// <summary>
        /// Configure the log file and its overall information
        /// </summary>
        /// <param name="logFile"></param>
        static public void Initialize(string logFile)
        {
            if (string.IsNullOrEmpty(logFile)) { throw new ArgumentNullException(nameof(logFile)); }

            FilePath = logFile;
            File.WriteAllText(FilePath, "[\n");
            ProcessId = Process.GetCurrentProcess().Id;

            AddProcessInfo();
        }

        /// <summary>
        /// Access the file path currently used
        /// </summary>
        public static string FilePath { get; private set; }

        /// <summary>
        /// Access the Process id currently used
        /// </summary>
        public static int ProcessId { get; private set; }


        /// <summary>
        /// Enable or Pause tracing
        /// </summary>
        public static bool IsEnabled { get; set; }

        private static void AddProcessInfo()
        {
            lock (FilePath)
            {
                File.AppendAllText(FilePath,
                    $"{{ \"name\": \"process_name\", \"ph\": \"M\", \"pid\": {ProcessId}, \"cat\": \"__metadata\", \"tid\":0,\"ts\":0, \"args\": {{ \"name\": \"{Process.GetCurrentProcess().ProcessName}\" }} }},\n");
            }
        }

        /// <summary>
        /// Add thread id into the trace file
        /// </summary>
        /// <param name="threadId"></param>
        /// <param name="name"></param>
        /// <remarks><see cref="Initialize"/> must be called before</remarks>
        public static void IdentifyThread(int threadId, string name)
        {
            lock (FilePath)
            {
                File.AppendAllText(FilePath,
                    $"{{ \"name\": \"thread_name\", \"ph\": \"M\", \"pid\": {ProcessId}, \"cat\": \"__metadata\", \"tid\":{threadId},\"ts\":0, \"args\": {{ \"name\": \"{name}\" }} }},\n");
            }
        }

        /// <summary>
        ///  Add thread order into the trace file
        /// </summary>
        /// <param name="threadId"></param>
        /// <param name="order"></param>
        /// <remarks><see cref="Initialize"/> must be called before</remarks>
        public static void SetThreadOrder(int threadId, int order)
        {
            File.AppendAllText(FilePath,
                $"{{ \"name\": \"thread_sort_index\", \"ph\": \"M\", \"pid\": {ProcessId}, \"cat\": \"__metadata\", \"tid\":{threadId},\"ts\":0, \"args\": {{ \"sort_index\": \"{order}\" }} }},\n");
        }

        /// <summary>
        /// Add completed event for the current thread
        /// </summary>
        /// <param name="threadId">thread id</param>
        /// <param name="name">event name</param>
        /// <param name="duration">duration (microseconds)</param>
        /// <param name="timeStamp">timestamp (microseconds)</param>
        /// <param name="context">extra context</param>
        /// <remarks><see cref="Initialize"/> must be called before</remarks>
        public static void AddCompletedEvent(int threadId, string name, long duration, long timeStamp, string context)
        {
            if (IsEnabled)
            {
                lock (FilePath)
                {
                    var args = string.IsNullOrEmpty(context) ? string.Empty : $", \"args\": {{ \"Context\": \"{context}\"}}";
                    File.AppendAllText(FilePath,
                    $"{{\"name\": \"{name}\", \"ph\": \"X\" , \"ts\": {timeStamp}, \"dur\": {duration},\"pid\": {ProcessId}, \"tid\": {threadId} {args}}},\n");
                }
            }
        }

        /// <summary>
        /// Add completed event for the current thread
        /// </summary>
        /// <param name="threadId">thread id</param>
        /// <param name="name">event name</param>
        /// <param name="timeStamp">timestamp (microseconds)</param>
        /// <param name="context">extra context</param>
        /// <remarks><see cref="Initialize"/> must be called before</remarks>
        public static void AddBeginEvent(int threadId, string name, long timeStamp, string context)
        {
            if (IsEnabled)
            {
                lock (FilePath)
                {
                    var args = string.IsNullOrEmpty(context) ? string.Empty : $", \"args\": {{ \"Context\": \"{context}\"}}";
                    File.AppendAllText(FilePath,
                    $"{{\"name\": \"{name}\", \"ph\": \"B\" , \"ts\": {timeStamp}, \"pid\": {ProcessId}, \"tid\": {threadId} {args}}},\n");
                }
            }
        }

        /// <summary>
        /// Add completed event for the current thread
        /// </summary>
        /// <param name="threadId">thread id</param>
        /// <param name="name">event name</param>
        /// <param name="timeStamp">timestamp (microseconds)</param>
        /// <param name="context">extra context</param>
        /// <remarks><see cref="Initialize"/> must be called before</remarks>
        public static void AddEndEvent(int threadId, string name, long timeStamp, string context)
        {
            if (IsEnabled)
            {
                lock (FilePath)
                {
                    var args = string.IsNullOrEmpty(context) ? string.Empty : $", \"args\": {{ \"Context\": \"{context}\"}}";
                    File.AppendAllText(FilePath,
                    $"{{\"name\": \"{name}\", \"ph\": \"E\" , \"ts\": {timeStamp}, \"pid\": {ProcessId}, \"tid\": {threadId} {args}}},\n");
                }
            }
        }
    }
}
