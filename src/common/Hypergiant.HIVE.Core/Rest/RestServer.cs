using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hypergiant.HIVE.Rest
{
    public class RestServer : IDisposable
    {
        private TcpListener m_tcp;
        private bool m_shutDown = false;

        private Dictionary<string, Dictionary<string, Func<HttpContext, HttpResponse>>> m_handlers
            = new Dictionary<string, Dictionary<string, Func<HttpContext, HttpResponse>>>();

        private ILogService LogService { get; set; }

        public RestServer(int port, ILogService logService = null)
        {
            m_tcp = new TcpListener(IPAddress.Any, port);
            LogService = logService;
        }

        public void Dispose()
        {
            m_tcp?.Stop();
        }

        public void Start()
        {
            m_tcp.Start();

            Task.Run(() => ConnectionListenerProc());
        }

        public void AddHandler(string verb, string pathRegex, Func<HttpContext, HttpResponse> handler)
        {
            LogService?.Information($"Adding REST Endpoint: {verb} => {pathRegex}");

            lock (m_handlers)
            {
                if (!m_handlers.ContainsKey(verb.ToUpper()))
                {
                    // new verb
                    m_handlers.Add(verb.ToUpper(), new Dictionary<string, Func<HttpContext, HttpResponse>>());
                }

                if (!m_handlers[verb.ToUpper()].ContainsKey(pathRegex))
                {
                    m_handlers[verb.ToUpper()].Add(pathRegex, handler);
                }
                else
                {
                    throw new Exception($"{verb} to '{pathRegex}' already registered");
                }
            }
        }

        private async Task ConnectionListenerProc()
        {
            while (!m_shutDown)
            {
                var client = await m_tcp.AcceptTcpClientAsync();

                Debug.WriteLine($"Client has connected from {client.Client.RemoteEndPoint}");

                // TODO: log

                // get the request data
                HttpRequest request = null;
                HttpResponse errorResponse = null;
                try
                {
                    request = await HttpRequest.From(client);
                }
                catch (Exception ex)
                {
                    errorResponse = HttpResponse.ServerError(ex.Message);
                }

                var context = new HttpContext(client, request);
                if (context.Request.Verb == null)
                {
                    // TODO: invalid http request? Handle this
                }
                else
                {
                    if (errorResponse != null)
                    {
                        errorResponse.Send(context);
                        await Task.Delay(500);
                        client?.Dispose();
                    }
                    else
                    {
                        _ = Task.Run(() =>
                        {
                            HandleRequest(context);
                            client?.Dispose();
                        });
                    }
                }
            }
        }

        private void HandleRequest(HttpContext context)
        {
            HttpResponse response = null;
            try
            {
                if (!m_handlers.ContainsKey(context.Request.Verb))
                {
                    response = HttpResponse.NotFound();
                }
                else
                {

                    foreach (var r in m_handlers[context.Request.Verb])
                    {
                        // look for a regex match
                        if (Regex.IsMatch(context.Request.Path, r.Key))
                        {
                            response = r.Value(context);
                            break;
                        }
                    }
                }
                if (response == null)
                {
                    response = HttpResponse.NotFound();
                }
            }
            catch (Exception e)
            {
                LogService?.Error($"Error handling request: {e.Message}");
                response = HttpResponse.ServerError(e.Message);
            }
            response.Send(context);
        }

        public void Stop()
        {
            m_tcp.Stop();
        }
    }
}
