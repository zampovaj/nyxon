using System.Net.Http.Json;

namespace Nyxon.Client.Services
{
    public class AntiforgeryHandler : DelegatingHandler
    {
        private string? _requestToken;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_requestToken == null)
            {
                //cretae temp client to prevent infinite loops
                var tokenClient = new HttpClient
                {
                    BaseAddress = request
                        .RequestUri != null ?
                            new Uri(request.RequestUri.GetLeftPart(UriPartial.Authority))
                            : null
                };

                var response = await tokenClient.GetFromJsonAsync<CsrfTokenResponse>("api/auth/csrf", cancellationToken);
                _requestToken = response?.Token;
            }

            // if the rewuest changes data add header
            if (request.Method == HttpMethod.Post
                || request.Method == HttpMethod.Put
                || request.Method == HttpMethod.Delete)
            {
                if (!string.IsNullOrEmpty(_requestToken))
                {
                    request.Headers.Add("X-CSRF-TOKEN", _requestToken);
                }
            }
            
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