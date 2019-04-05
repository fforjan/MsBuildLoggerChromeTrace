using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MsBuildLoggerChromeTrace
{
    public class ChromeTraceLogger : Logger
    {
        public override void Initialize(Microsoft.Build.Framework.IEventSource eventSource)
        {
            // The name of the log file should be passed as the first item in the
            // "parameters" specification in the /logger switch.  It is required
            // to pass a log file to this logger. Other loggers may have zero or more than 
            // one parameters.
            if (null == Parameters)
            {
                throw new LoggerException("Log file was not set");
            }
            string[] parameters = Parameters.Split(';');
            
            string logFile = parameters[0];
            if (String.IsNullOrEmpty(logFile))
            {
                throw new LoggerException("Log file was not set.");
            }
            
            if (parameters.Length > 1)
            {
                throw new LoggerException("Too many parameters passed.");
            }

            Console.WriteLine($"using '{logFile}' as log file");
            
            ChromeTracer.Initialize(logFile);

            if (Verbosity >= LoggerVerbosity.Diagnostic)
            {
                eventSource.TaskStarted += TaskStarted;
                eventSource.TaskStarted += TaskFinished;
            }

            if (Verbosity >= LoggerVerbosity.Detailed)
            {
                eventSource.TargetStarted += TargetStarted;
                eventSource.TargetFinished += TargetFinished;
            }
            
            if (Verbosity >= LoggerVerbosity.Normal)
            {
                eventSource.ProjectStarted += ProjectStarted;
                eventSource.ProjectFinished += ProjectFinished;
            }
            
        }

        private void TaskStarted(object sender, TaskStartedEventArgs e)
        {
            ChromeTracer.AddBeginEvent(e.ThreadId, "Task:" + e.TaskName, GetTimeStamp(e.Timestamp), string.Empty);
        }

        private void TaskFinished(object sender, TaskStartedEventArgs e)
        {
            ChromeTracer.AddEndEvent(e.ThreadId, "Task:" + e.TaskName, GetTimeStamp(e.Timestamp), string.Empty);
        }

        private void TargetFinished(object sender, Microsoft.Build.Framework.TargetFinishedEventArgs e)
        {
            ChromeTracer.AddEndEvent(e.ThreadId, "Target:" + e.TargetName, GetTimeStamp(e.Timestamp), string.Empty);
        }

        private void TargetStarted(object sender, Microsoft.Build.Framework.TargetStartedEventArgs e)
        {
            ChromeTracer.AddBeginEvent(e.ThreadId, "Target:" +e.TargetName, GetTimeStamp(e.Timestamp), string.Empty);
        }

        private void ProjectStarted(object sender, Microsoft.Build.Framework.ProjectStartedEventArgs e)
        {
            ChromeTracer.AddBeginEvent(e.ThreadId, "Project:" + Path.GetFileName(e.ProjectFile), GetTimeStamp(e.Timestamp), string.Empty);
        }

        private void ProjectFinished(object sender, Microsoft.Build.Framework.ProjectFinishedEventArgs e)
        {
            ChromeTracer.AddEndEvent(e.ThreadId, "Project:" + Path.GetFileName(e.ProjectFile), GetTimeStamp(e.Timestamp), string.Empty);
        }

        private long GetTimeStamp(DateTime timestamp)
        {

            return (long) (1000000.0 * (timestamp.Ticks /* System.Diagnostics.Stopwatch.GetTimestamp() */  + .0) / System.Diagnostics.Stopwatch.Frequency);
        }
    }
}
