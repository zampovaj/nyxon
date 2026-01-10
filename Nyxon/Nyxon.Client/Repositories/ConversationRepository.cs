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

        public async Task<PrekeyBundleResponse?> GetPrekeyBundle(string username)
        {
            try
            {
                return await _apiService.GetAsync<PrekeyBundleResponse>($"api/prekeys/{username}");
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
                return await _apiService.GetAsync<ConversationVaultDto>($"api/conversation/vault/{conversationId}");
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
        public async Task<CreateConversationResponse?> CreateConversationAsync(CreateConversationRequest conversationRequest)
        {
            try
            {
                return await _apiService.PostAsync<CreateConversationResponse, CreateConversationRequest>("api/conversation", conversationRequest);
            }
            catch
            {
                return null;
            }
        }

        public async Task<SendMessageResponse?> SendMessageAsync(SendMessageRequest messageRequest)
        {
            try
            {
                return await _apiService.PostAsync<SendMessageResponse, SendMessageRequest>("api/message/send", messageRequest);
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> CreateConversationVaultAsync(Guid conversationId, ConversationVaultData vaultData)
        {
            try
            {
                await _apiService.PostAsync<ConversationVaultData>($"/api/conversation/vault/{conversationId}", vaultData);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during saving conversation vault: {ex.Message}");
                return false;
            }
        }

        public async Task<MessageResponse?> GetMessageAsync(string kvKey)
        {
            try
            {
                return await _apiService.GetAsync<MessageResponse>($"api/message/{kvKey}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during fetching message: {ex.Message}");
                return null;
            }
        }
        public async Task<MessageReceivedStateUpdateResponse?> ReceiveMessageServerUpdateAsync(MessageReceivedStateUpdateRequest request)
        {
            try
            {
                return await _apiService.PatchAsync<MessageReceivedStateUpdateResponse, MessageReceivedStateUpdateRequest>("api/message/receive", request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during receive server update: {ex.Message}");
                return null;
            }
        }
    }
}