﻿using System;

namespace IctBaden.Stonehenge2.Katana
{
    using System.Diagnostics;

    using Hosting;
    using Resources;

    using Microsoft.Owin.Hosting;

    public class KatanaHost : IStonehengeHost
    {
        private IDisposable _webApp;

        private readonly IStonehengeResourceProvider _resourceLoader;

        public KatanaHost(IStonehengeResourceProvider loader)
        {
            _resourceLoader = loader;
        }

        public string AppTitle { get; private set; }

        public string BaseUrl { get; private set; }

        public bool Start(string title, bool useSsl, string hostAddress, int hostPort)
        {
            AppTitle = title;

            try
            {
                BaseUrl = (useSsl ? "https://" : "http://") 
                    + (hostAddress ?? "*" )
                    + ":" 
                    + (hostPort != 0 ? hostPort : (useSsl ? 443 : 80));

                var startup = new Startup(title, _resourceLoader);
                _webApp = WebApp.Start(BaseUrl, startup.Configuration);
            }
            catch (Exception ex)
            {
                if (ex is MissingMemberException && ex.Message.Contains("Microsoft.Owin.Host.HttpListener"))
                {
                    Trace.TraceError("Missing reference to nuget package 'Microsoft.Owin.Host.HttpListener'");
                }
                Trace.TraceError("KatanaHost.Start: " + ex.Message);
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    Trace.TraceError(" + " + ex.Message);
                }
            }
            return _webApp != null;
        }

        public void Terminate()
        {
            _webApp?.Dispose();
            _webApp = null;
        }
    }
}
