using System.Collections.Generic;
using IctBaden.Stonehenge2.Core;
using Microsoft.Owin.Diagnostics;
using SqueezeMe;

namespace IctBaden.Stonehenge2.Katana
{
    using Middleware;
    using Resources;

    using Owin;

    internal class Startup
    {
        private readonly string _appTitle;
        private readonly IStonehengeResourceProvider _resourceLoader;
        private readonly List<AppSession> _appSessions = new List<AppSession>();
        private readonly bool _disableSessionIdUrlParameter;

        public Startup(string title, IStonehengeResourceProvider loader, bool disableSessionIdUrlParameter)
        {
            _appTitle = title;
            _resourceLoader = loader;
            _disableSessionIdUrlParameter = disableSessionIdUrlParameter;
        }

        public void Configuration(IAppBuilder app)
        {
#if DEBUG
            var errorOptions = new ErrorPageOptions
            {
                ShowExceptionDetails = true,
                ShowSourceCode = true
            };
            app.UseErrorPage(errorOptions);
#endif
            app.Use<ServerExceptionLogger>();
            app.Use<StonehengeAcme>();
            app.UseCompression();
            app.Use((context, next) =>
            {
                context.Environment.Add("stonehenge.DisableSessionIdUrlParameter", _disableSessionIdUrlParameter);
                context.Environment.Add("stonehenge.AppTitle", _appTitle);
                context.Environment.Add("stonehenge.ResourceLoader", _resourceLoader);
                context.Environment.Add("stonehenge.AppSessions", _appSessions);
                return next.Invoke();
            });
            app.Use<StonehengeSession>();
            app.Use<StonehengeHeaders>();
            app.Use<StonehengeRoot>();
            app.Use<StonehengeContent>();
        }

    }
}