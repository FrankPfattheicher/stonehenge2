﻿using NUnit.Framework;

namespace IctBaden.Stonehenge2.Test.Hosting
{
    using System;
    using System.Diagnostics;
    using Katana;
    using Tools;

    // ReSharper disable InconsistentNaming
    [TestFixture]
    public class HostingTests
    {

        [Test]
        public void Host_StartupOk_RespondsOnHttpRequest()
        {
            const string content = "<h1>Test</h1>";

            var loader = new TestResourceLoader(content);
            var host = new KatanaHost(loader);

            var startOk = host.Start("Test", false, "localhost", 32001);
            Assert.IsTrue(startOk, "Start failed");

            var response = string.Empty;
            try
            {
                using (var client = new RedirectableWebClient())
                {
                    response = client.DownloadString(host.BaseUrl);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            Assert.AreEqual(content, response);
            host.Terminate();
        }

        [Test]
        public void Host_MultipleInstances_StartupOk_RespondsOnHttpRequest()
        {
            const string content1 = "<h1>Test 01</h1>";
            const string content2 = "<h1>Test II</h1>";

            var loader1 = new TestResourceLoader(content1);
            var host1 = new KatanaHost(loader1);

            var startOk = host1.Start("Test", false, "localhost", 32002);
            Assert.IsTrue(startOk, "Start host1 failed");

            var loader2 = new TestResourceLoader(content2);
            var host2 = new KatanaHost(loader2);

            startOk = host2.Start("Test", false, "localhost", 32003);
            Assert.IsTrue(startOk, "Start host2 failed");

            Assert.AreNotEqual(host1.BaseUrl, host2.BaseUrl);

            var response1 = string.Empty;
            var response2 = string.Empty;
            try
            {
                using (var client = new RedirectableWebClient())
                {
                    response1 = client.DownloadString(host1.BaseUrl);
                }
                using (var client = new RedirectableWebClient())
                {
                    response2 = client.DownloadString(host2.BaseUrl);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Assert.Fail(ex.Message);
            }

            Assert.AreEqual(content1, response1);
            Assert.AreEqual(content2, response2);

            host1.Terminate();
            host2.Terminate();
        }
    
    }
}
