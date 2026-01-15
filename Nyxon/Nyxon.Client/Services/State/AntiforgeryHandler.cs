using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace Nyxon.Client.Services.State
{
    public class AntiforgeryHandler : DelegatingHandler
    {
        private readonly CsrfTokenStore _csrfTokenStore;

        public AntiforgeryHandler(CsrfTokenStore csrfTokenStore)
        {
            _csrfTokenStore = csrfTokenStore;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await _csrfTokenStore.Lock.WaitAsync(cancellationToken);
            Console.WriteLine("Antiforgery check running");
            try
            {
                if (_csrfTokenStore.Token == null)
                {
                    var timestamp = DateTime.UtcNow.Ticks;
                    var csrfRequest = new HttpRequestMessage(
                        HttpMethod.Get,
                        $"api/auth/csrf?t={timestamp}" // Keep the cache buster!
                    );

                    // 2. 🔥 CRITICAL: Tell Browser to send Cookies (Credentials)
                    csrfRequest.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

                    //cretae temp client to prevent infinite loops
                    var tokenClient = new HttpClient
                    {
                        BaseAddress = request
                            .RequestUri != null ?
                                new Uri(request.RequestUri.GetLeftPart(UriPartial.Authority))
                                : null
                    };

                    var response = await tokenClient.GetFromJsonAsync<CsrfTokenResponse>("api/auth/csrf", cancellationToken);
                    _csrfTokenStore.Token = response?.Token;
                }
            }
            finally
            {
                _csrfTokenStore.Lock.Release();
            }

            // if the rewuest changes data add header
            if (request.Method == HttpMethod.Post
                || request.Method == HttpMethod.Put
                || request.Method == HttpMethod.Delete)
            {
                if (!string.IsNullOrEmpty(_csrfTokenStore.Token))
                {
                    request.Headers.Add("X-CSRF-TOKEN", _csrfTokenStore.Token);
                }
            }
            Console.WriteLine("Antiforgery check finished");
            _csrfTokenStore.Check();

            // otherwise return normally
            return await base.SendAsync(request, cancellationToken);
        }

        // helper model
        private class CsrfTokenResponse
        {
            public string Token { get; set; } = string.Empty;
        }
    }
}