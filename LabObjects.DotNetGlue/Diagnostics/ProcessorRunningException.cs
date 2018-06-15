using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LabObjects.DotNetGlue.Diagnostics
{
    internal class ProcessorRunningException: Exception
    {
        public ProcessorRunningException() : base("Process is Running") { }
        public ProcessorRunningException(IProcessor processor) 
            : base($"Process is Running: {processor.ProcessName}, Id={processor.ProcessId}") { }

    }
}
