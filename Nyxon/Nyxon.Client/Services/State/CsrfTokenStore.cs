using System.Threading;

namespace Nyxon.Client.Services.State
{
    public class CsrfTokenStore
    {
        public string? Token { get; set; }
        // this should fix that fucking csrf bug with stale tokens i spent 4 hours on
        public SemaphoreSlim Lock { get; } = new SemaphoreSlim(1, 1);

        public void Clear()
        {
            Token = null;
            Console.WriteLine("Clearing token");
        }

        public void Check()
        {
            Console.WriteLine("Token: " + (Token ?? "null"));
        }
    }
}