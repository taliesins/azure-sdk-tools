using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.WindowsAzure.Storage.DataMovement.Tests
{
    [TestClass]
    public class GlobalMemoryStatusNativeMethodsTests
    {
        private GlobalMemoryStatusNativeMethods globalMemoryStatusNativeMethod;
         
        [TestInitialize]
        public void InitTest()
        {
            globalMemoryStatusNativeMethod = new GlobalMemoryStatusNativeMethods();
        }

        [TestCleanup]
        public void CleanTest()
        {
            globalMemoryStatusNativeMethod = null;
        }

        [TestMethod]
        public void ReturnsExpectedMaximumMemory()
        {
            var maximumCacheSize = Math.Min((long)((double)((float)globalMemoryStatusNativeMethod.AvailablePhysicalMemory) * 0.5), (long)int.MaxValue);

            Assert.IsTrue(maximumCacheSize > 4000000);
            Assert.IsTrue(maximumCacheSize <= int.MaxValue);
        }

    }
}
