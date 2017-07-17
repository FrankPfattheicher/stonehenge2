﻿using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace IctBaden.Stonehenge2.Katana.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.Owin;

    public class StonehengeHeaders
    {
        private readonly Func<IDictionary<string, object>, Task> _next;
        private static Dictionary<string, string> _headers;

        public StonehengeHeaders(Func<IDictionary<string, object>, Task> next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            IOwinContext context = new OwinContext(environment);

            try
            {
                if (_headers == null)
                    LoadHeaders();
                // ReSharper disable once PossibleNullReferenceException
                foreach (var header in _headers)
                {
                    context.Response.Headers.Set(header.Key, header.Value);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error handling default headers: " + ex.Message);
            }
            await _next.Invoke(environment);
        }

        private void LoadHeaders()
        {
            _headers = new Dictionary<string, string>();

            var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? ".";
            var headersFile = Path.Combine(path, "defaultheaders.txt");
            if (!File.Exists(headersFile)) return;

            Trace.TraceInformation("Adding default headers from: " + headersFile);
            var headers = File.ReadAllLines(headersFile);
            foreach (var header in headers)
            {
                if (string.IsNullOrEmpty(header)) continue;
                if (header.StartsWith("#")) continue;

                var colon = header.IndexOf(':');
                if (colon < 1) continue;
                var key = header.Substring(0, colon).Trim();
                var value = header.Substring(colon + 1).Trim();
                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    Trace.TraceInformation($"Add header: {key}: {value}");
                    _headers.Add(key, value);
                }
            }
        }
    }
}
