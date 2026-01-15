using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Nyxon.Client.Services.State
{
    public class Http401Handler : DelegatingHandler
    {
        private readonly HostAuthenticationStateProvider _authStateProvider;
        private readonly NavigationManager _nav;

        public Http401Handler(HostAuthenticationStateProvider authStateProvider, NavigationManager nav)
        {
            _authStateProvider = authStateProvider;
            _nav = nav;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("Unauthorized!");
                ((HostAuthenticationStateProvider)_authStateProvider).NotifyUnauthorized();
                _nav.NavigateTo("/", true);
            }

            return response;
        }
    }

}