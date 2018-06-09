using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using LabObjects.DotNetGlue.Diagnostics;

namespace LabObjects.DotNetGlue.Tests
{
    [TestClass]
    public class Processor_RunHiddenTests
    {
 

        [TestMethod]
        public void Processor_RunCurlGetWebPage()
        {
            try
            {
  

                HostProcessor host = new HostProcessor();
                Processor p = host.CreateProcessor();
                //p.
                p.FileName = "c:\\bin\\curl.exe";
                //p.Arguments = "http://example.com";
               p.Arguments = "http://labobjects.com";
         
                if (p.RunHidden())
                {
                    int exitCode = p.ExitCode;
                    string s = p.Output;
                    string errMsg = p.ErrorOutput;

                    Assert.AreEqual(p.ExitCode, 0);
                    Assert.IsNotNull(p.Output);
                    Assert.IsFalse(p.DidTimeout);
                    Assert.IsFalse(p.IsRunning);
                    //Assert.IsTrue(p.IsStarted);
                }
                else
                {
                    Assert.Fail(p.LastError);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void Processor_RunHidden_CmdWindowDir()
        {
            try
            {
                //FileVersionInfo fvi = new FileVersionInfo("filename");
                HostProcessor host = new HostProcessor();
                Processor p = host.CreateProcessor();
                p.FileName = "cmd.exe";
                p.Arguments = "/C dir /B " + Environment.CurrentDirectory;

                if (p.RunHidden())
                {
                    int exitCode = p.ExitCode;
                    string s = p.Output;
                    string errMsg = p.ErrorOutput;

                    Assert.AreEqual(p.ExitCode, 0);
                    Assert.IsFalse(p.DidTimeout);
                    Assert.IsNotNull(p.Output);
                    Assert.IsFalse(p.DidTimeout);
                    Assert.IsFalse(p.IsRunning);
                    //Assert.IsTrue(p.IsStarted);
                }
                else
                {
                    Assert.Fail(p.LastError);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void Processor_RunHidden_ChoiceYesNo()
        {
            try
            {
                HostProcessor host = new HostProcessor();
                Processor p = host.CreateProcessor();
                p.FileName = "cmd.exe";
                p.Arguments = "/C choice";

                if (p.RunHidden("Y"))
                {
                    int exitCode = p.ExitCode;
                    string s = p.Output;
                    string errMsg = p.ErrorOutput;

                    Assert.AreEqual(p.ExitCode, 1);
                    Assert.IsFalse(p.DidTimeout);
                    Assert.IsNotNull(p.Output);
                    Assert.IsFalse(p.IsRunning);
                }
                else
                {
                    Assert.Fail(p.LastError);
                }
                if (p.RunHidden("N"))
                {
                    int exitCode = p.ExitCode;
                    string s = p.Output;
                    string errMsg = p.ErrorOutput;

                    Assert.AreEqual(p.ExitCode, 2);
                    Assert.IsFalse(p.DidTimeout);
                    Assert.IsNotNull(p.Output);
                    Assert.IsFalse(p.IsRunning);
                }
                else
                {
                    Assert.Fail(p.LastError);
                }
           

            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void Processor_RunHidden_ChoiceNo()
        {
            try
            {
                //FileVersionInfo fvi = new FileVersionInfo("filename");
                HostProcessor host = new HostProcessor();
                Processor p = host.CreateProcessor();
                p.FileName = "cmd.exe";
                p.Arguments = "/C choice";

                if (p.RunHidden("N"))
                {
                    int exitCode = p.ExitCode;
                    string s = p.Output;
                    string errMsg = p.ErrorOutput;

                    Assert.AreEqual(p.ExitCode, 2);
                    Assert.IsFalse(p.DidTimeout);
                    Assert.IsNotNull(p.Output);
                    Assert.IsFalse(p.IsRunning);
                    Assert.AreEqual("No Error", p.LastError);
                }
                else
                {
                    Assert.Fail(p.LastError);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

    }
}
