using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly IApiService _apiService;

        public ConversationRepository(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<PrekeyBundleResponse?> GetPrekeyBundle()
        {
            try
            {
                return await _apiService.GetAsync<PrekeyBundleResponse>("api/prekeys");
            }
            catch
            {
                return null;
            }
        }

        public async Task<ConversationVaultDto?> FetchVaultAsync(Guid conversationId)
        {
            try
            {
                return await _apiService.GetAsync<ConversationVaultDto>($"api/conversation/vaults/{conversationId}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<InboxDto?> FetchInboxAsync()
        {
            try
            {
                return await _apiService.GetAsync<InboxDto>($"api/conversation/inbox");
            }
            catch
            {
                return null;
            }
        }
    }
}