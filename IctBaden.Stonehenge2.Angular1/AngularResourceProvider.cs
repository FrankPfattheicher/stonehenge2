﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using IctBaden.Stonehenge2.Angular.Client;
using IctBaden.Stonehenge2.Core;
using IctBaden.Stonehenge2.Resources;

namespace IctBaden.Stonehenge2.Angular1
{
  public class AngularResourceProvider : IStonehengeResourceProvider
    {
        private Dictionary<string, Resource> _angularContent;

        public void InitProvider(StonehengeResourceLoader loader, string appTitle, string rootPage) => InitProvider(loader, appTitle, rootPage, null);
        public void InitProvider(StonehengeResourceLoader loader, string appTitle, string rootPage, string appFilesPath)
        {
            _angularContent = new Dictionary<string, Resource>();

            var resLoader = (ResourceLoader)loader.Loaders.First(ld => ld.GetType() == typeof(ResourceLoader));
            resLoader.AddAssembly(typeof(AngularResourceProvider).Assembly);
            loader.Loaders.Add(this);

            AddFileSystemContent(appFilesPath);
            AddResourceContent();
            AddAppJs(rootPage);
        }

        public void Dispose()
        {
            _angularContent.Clear();
        }

        private static ViewModelInfo GetViewModelInfo(string route, string pageText)
        {
            var info = new ViewModelInfo(route.Substring(0, 1).ToUpper() + route.Substring(1) + "Vm");

            var extractName = new Regex("ng-controller=\\\"(\\w+)\\\"");
            var match = extractName.Match(pageText);
            if (match.Success)
            {
                info.VmName = match.Groups[1].Value;
            }
            return info;
        }

        private void AddFileSystemContent(string appFilesPath)
        {
            if (Directory.Exists(appFilesPath))
            {
                var appPath = Path.DirectorySeparatorChar + "app" + Path.DirectorySeparatorChar;
                var appFiles = Directory.GetFiles(appFilesPath, "*.html", SearchOption.AllDirectories);

                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var appFile in appFiles)
                {
                    var resourceId = appFile.Substring(appFile.IndexOf(appPath, StringComparison.InvariantCulture) + 5).Replace('\\', '/');
                    var route = resourceId.Replace(".html", string.Empty);
                    var pageText = File.ReadAllText(appFile);

                    var resource = new Resource(route, appFile, ResourceType.Html, pageText, Resource.Cache.Revalidate) { ViewModel = GetViewModelInfo(route, pageText) };
                    _angularContent.Add(resourceId, resource);
                }
            }
        }

        private void AddResourceContent()
        {
            var assembly = Assembly.GetEntryAssembly();
            var baseName = assembly.GetName().Name + ".app.";

            foreach (var resourceName in assembly.GetManifestResourceNames()
              .Where(name => (name.EndsWith(".html")) && (!name.Contains("index.html"))).OrderBy(name => name))
            {
                var resourceId = resourceName.Substring(baseName.Length);
                if (_angularContent.ContainsKey(resourceId))
                {
                    Trace.TraceWarning("AngularResourceProvider.AddResourceContent: Resource with id {0} already exits", resourceId);
                    continue;
                }

                var route = resourceId.Replace(".html", string.Empty);
                var pageText = "";
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            pageText = reader.ReadToEnd();
                        }
                    }
                }

                var resource = new Resource(route, "res://" + resourceName, ResourceType.Html, pageText, Resource.Cache.Revalidate) { ViewModel = GetViewModelInfo(route, pageText) };
                _angularContent.Add(resourceId, resource);
            }
        }

        private void AddAppJs(string rootPage)
        {
            var appCreator = new AngularAppCreator(rootPage, _angularContent);
            var resource = new Resource("stonehengeApp.js", "AngularResourceProvider", ResourceType.Html,
                appCreator.CreateApplicationJs(), Resource.Cache.Revalidate);
            _angularContent.Add("stonehengeApp.js", resource);
        }


        public Resource Post(AppSession session, string resourceName, Dictionary<string, string> parameters, Dictionary<string, string> formData)
        {
            return null;
        }
        public Resource Get(AppSession session, string resourceName, Dictionary<string, string> parameters)
        {
            if (_angularContent.ContainsKey(resourceName))
            {
                return _angularContent[resourceName];
            }
            return null;
        }
    }
}
