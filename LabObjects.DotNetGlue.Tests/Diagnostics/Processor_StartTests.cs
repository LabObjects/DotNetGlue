using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using LabObjects.DotNetGlue.Diagnostics;

namespace LabObjects.DotNetGlue.Tests
{
    [TestClass]
    public class ProcessorStartTests
    {

        [TestMethod]
        public void Test_CMDWindow()
        {
            //todo: wait for exit methid
            // input stream flush (or otehr helpers?
            // close
            HostProcessor host = new HostProcessor();
            Processor processor = host.CreateProcessor();

            processor.FileName = "cmd.exe";
            processor.RedirectStandardError = true;
            processor.RedirectStandardOutput = true;
            processor.RedirectStandardInput = true;

            processor.WindowStyle = ProcessWindowStyle.Hidden;
            processor.Start();

            processor.InputWriteLine("cd c:\\code");
            processor.InputWriteLine("dir /b *.*");
            if (processor.IsOutputAvailable)
            {
                string so = processor.OutputRead();
            }
            if (processor.IsErrorAvailable)
            {
                string se = processor.ErrorOutputRead();
            }
            processor.InputWriteLine("exit");

            while (!processor.HasExited)
            {
                
            }

            processor.Close();

            if ( host.HasActiveProcessors )
            {

            }

        }
    }
}
