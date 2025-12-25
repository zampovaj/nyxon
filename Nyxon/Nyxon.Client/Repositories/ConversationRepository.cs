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

        public async Task<List<ConversationSummaryDto>>? FetchInboxAsync()
        {
            try
            {
                return await _apiService.GetAsync<List<ConversationSummaryDto>>("api/conversation/inbox");
            }
            catch
            {
                return null;
            }
        }
    }
}