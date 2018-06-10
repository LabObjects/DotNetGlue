using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LabObjects.DotNetGlue.Diagnostics;

namespace LabObjects.DotNetGlue.Tests
{
    [TestClass]
    public class HostProcessor_CoreTests
    {

        [TestMethod]
        public void HostProcessor_GetHost()
        {
            HostProcessor host = new HostProcessor();
            
            var productName = host.ProductName;
            Assert.AreEqual("Microsoft.TestHost.x86",productName);

        }
    }
}
