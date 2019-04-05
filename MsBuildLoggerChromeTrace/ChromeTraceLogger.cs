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
            ChromeTracer.Initialize("MSBuild.json");
            ChromeTracer.IsEnabled = true;


            eventSource.ProjectStarted += ProjectStarted; ;
            eventSource.ProjectFinished += ProjectFinished;

            eventSource.TargetStarted += TargetStarted;
            eventSource.TargetFinished += TargetFinished; ;
        }


        private void TargetFinished(object sender, Microsoft.Build.Framework.TargetFinishedEventArgs e)
        {
            ChromeTracer.AddEndEvent(e.ThreadId, e.TargetName, GetTimeStamp(e.Timestamp), string.Empty);
        }

        private void TargetStarted(object sender, Microsoft.Build.Framework.TargetStartedEventArgs e)
        {
            ChromeTracer.AddBeginEvent(e.ThreadId, e.TargetName, GetTimeStamp(e.Timestamp), string.Empty);
        }

        private void ProjectStarted(object sender, Microsoft.Build.Framework.ProjectStartedEventArgs e)
        {
            ChromeTracer.AddBeginEvent(e.ThreadId, Path.GetFileName(e.ProjectFile), GetTimeStamp(e.Timestamp), string.Empty);
        }

        private void ProjectFinished(object sender, Microsoft.Build.Framework.ProjectFinishedEventArgs e)
        {
            ChromeTracer.AddEndEvent(e.ThreadId, Path.GetFileName(e.ProjectFile), GetTimeStamp(e.Timestamp), string.Empty);
        }

        private long GetTimeStamp(DateTime timestamp)
        {
            return (long) (1000000.0 * (timestamp.Ticks + .0) / System.Diagnostics.Stopwatch.Frequency);
        }
    }
}
